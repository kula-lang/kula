using System.Text.RegularExpressions;

namespace Kula.Core;

class ModuleResolver {
    private KulaEngine? kula;
    private string? root;

    private ModuleResolver() { }
    public static ModuleResolver Instance = new ModuleResolver();

    public List<FileInfo> Resolve(KulaEngine kula, string root) {
        this.kula = kula;
        this.root = root;

        List<FileInfo> file_info_list = new List<FileInfo>();
        List<FileInfo[]> topo_info_list = new List<FileInfo[]>();

        foreach (string raw_filename in Directory.EnumerateFiles(root, "*.kula")) {
            FileInfo file = new FileInfo(raw_filename);
            DirectoryInfo directory = file.Directory!;

            string source = file.OpenText().ReadToEnd();
            FileInfo[] files = AnalyzeModule(directory, source);

            file_info_list.Add(file);
            topo_info_list.Add(files);

            // Console.WriteLine(file.FullName);
            // foreach (FileInfo file_info in files) {
            //     Console.WriteLine("\t" + file_info.FullName);
            // }
        }

        List<FileInfo> final_files = TopoSortFiles(file_info_list, topo_info_list);
        // foreach (FileInfo file_info in final_files) {
        //     Console.WriteLine(file_info.FullName);
        // }

        return final_files;
    }

    private List<FileInfo> TopoSortFiles(List<FileInfo> files, List<FileInfo[]> topo) {
        int[][] neighbors = new int[topo.Count][];
        for (int i = 0; i < topo.Count; ++i) {
            neighbors[i] = new int[topo[i].Length];
            for (int j = 0; j < topo[i].Length; ++j) {
                neighbors[i][j] = MyIndexOf(files, topo[i][j]);
            }
        }

        List<int> sorted = TopoSort(neighbors);
        List<FileInfo> ans = new List<FileInfo>();
        foreach (int i in sorted) {
            ans.Add(files[i]);
        }

        return ans;
    }

    private List<int> TopoSort(int[][] neighbor) {
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

    private FileInfo[] AnalyzeModule(DirectoryInfo directory, string source) {
        Regex rx = new Regex(@"^\s*import\s*\{(?<inner>.*?)\}", RegexOptions.Compiled | RegexOptions.Singleline);
        MatchCollection matches = rx.Matches(source);
        foreach (Match match in matches) {
            string inner = match.Groups["inner"].Value;
            string[] inner_values = inner.Trim().Split(',');
            for (int i = 0; i < inner_values.Length; ++i) {
                inner_values[i] = inner_values[i].Trim();
                inner_values[i] = inner_values[i].Substring(1, inner_values[i].Length - 2);
            }

            FileInfo[] files = new FileInfo[inner_values.Length];
            for (int i = 0; i < inner_values.Length; ++i) {
                files[i] = new FileInfo(directory.FullName + "/" + inner_values[i]);
            }
            return files;
        }

        return new FileInfo[0];
    }

    private static int MyIndexOf(List<FileInfo> arr, FileInfo item) {
        for (int i = 0; i < arr.Count; ++i) {
            if (item.FullName == arr[i].FullName) {
                return i;
            }
        }
        return -1;
    }
}