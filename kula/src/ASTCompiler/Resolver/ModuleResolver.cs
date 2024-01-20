using Kula.ASTCompiler.Lexer;
using Kula.ASTCompiler.Parser;

namespace Kula.ASTCompiler.Resolver;

class ModuleResolver
{
    HashSet<string> scannedFiles = new HashSet<string>();

    private ModuleResolver() { }
    public static ModuleResolver Instance = new ModuleResolver();

    public List<AstFile> Resolve(Dictionary<string, AstFile> fileDict, FileInfo root)
    {
        List<FileInfo> files = new List<FileInfo>();
        List<List<FileInfo>> nexts = new List<List<FileInfo>>();

        foreach (var kv in fileDict) {
            files.Add(kv.Value.fileInfo);
            nexts.Add(kv.Value.nexts);
        }

        List<string> sorted_file_names = TopoSortFiles(files, nexts);
        List<AstFile> sorted_files = new List<AstFile>();
        foreach (string fname in sorted_file_names) {
            sorted_files.Add(fileDict[fname]);
        }

        return sorted_files;
    }

    private List<string> TopoSortFiles(List<FileInfo> files, List<List<FileInfo>> topo)
    {
        int[][] neighbors = new int[topo.Count][];
        for (int i = 0; i < topo.Count; ++i) {
            neighbors[i] = new int[topo[i].Count];
            for (int j = 0; j < topo[i].Count; ++j) {
                neighbors[i][j] = MyIndexOf(files, topo[i][j]);
            }
        }

        List<int> sorted = TopoSort(neighbors);
        List<string> ans = new List<string>();
        foreach (int i in sorted) {
            ans.Add(files[i].FullName);
        }

        return ans;
    }

    private List<int> TopoSort(int[][] neighbor)
    {
        int len = neighbor.Length;
        HashSet<int>[] nexts = new HashSet<int>[len];
        int[] indegree = new int[len];

        Queue<int> que = new Queue<int>();
        for (int i = 0; i < len; ++i) {
            nexts[i] = new HashSet<int>();
        }
        for (int i = 0; i < len; ++i) {
            foreach (int j in neighbor[i]) {
                nexts[j].Add(i);
            }
            indegree[i] += neighbor[i].Length;
            if (indegree[i] == 0) {
                que.Enqueue(i);
            }
        }


        List<int> ans = new List<int>();
        while (que.Count != 0) {
            int last = que.Dequeue();
            foreach (int next in nexts[last]) {
                indegree[next] -= 1;
                if (indegree[next] == 0) {
                    que.Enqueue(next);
                }
            }
            ans.Add(last);
        }

        return ans;
    }

    public List<FileInfo> AnalyzeAST(DirectoryInfo directory, List<Stmt> asts)
    {
        List<FileInfo> nexts = new List<FileInfo>();
        foreach (var ast in asts) {
            if (ast is Stmt.Import import) {
                foreach (Token import_item in import.modules) {
                    nexts.Add(new FileInfo(directory.FullName + "/" + import_item.literial));
                }
            }
        }

        return nexts;
    }

    private static int MyIndexOf(List<FileInfo> arr, FileInfo item)
    {
        for (int i = 0; i < arr.Count; ++i) {
            if (item.FullName == arr[i].FullName) {
                return i;
            }
        }
        throw new Exception();
    }
}