using System.Text.RegularExpressions;

namespace Kula.Core;

class ModuleResolver {
    private KulaEngine? kula;
    private FileInfo? root;
    HashSet<string> scannedFiles = new HashSet<string>();

    private ModuleResolver() { }
    public static ModuleResolver Instance = new ModuleResolver();

    public List<FileInfo> Resolve(KulaEngine kula, FileInfo root) {
        this.kula = kula;
        this.root = root;

        (List<FileInfo> file_info_list, List<FileInfo[]> topo_info_list) = Scan(root);

        List<FileInfo> final_files = TopoSortFiles(file_info_list, topo_info_list);
        // Console.WriteLine(":");
        // foreach (FileInfo file_info in final_files) {
        //     Console.WriteLine(file_info.FullName);
        // }

        return final_files;
    }

    private (List<FileInfo>, List<FileInfo[]>) Scan(FileInfo root) {
        List<FileInfo> file_info_list = new List<FileInfo>();
        List<FileInfo[]> topo_info_list = new List<FileInfo[]>();

        Queue<FileInfo> file_info_que = new Queue<FileInfo>();
        file_info_que.Enqueue(root);

        while (file_info_que.Count > 0) {
            FileInfo file_info = file_info_que.Dequeue();

            if (!scannedFiles.Contains(file_info.FullName)) {
                string source = file_info.OpenText().ReadToEnd();
                FileInfo[] next_list = AnalyzeModule(file_info.Directory!, source);
                foreach (FileInfo next in next_list) {
                    file_info_que.Enqueue(next);
                }

                file_info_list.Add(file_info);
                topo_info_list.Add(next_list);

                scannedFiles.Add(file_info.FullName);
            }
        }

        // for (int i = 0; i < file_info_list.Count; ++i) {
        //     Console.WriteLine(file_info_list[i].FullName);
        //     foreach (FileInfo item in topo_info_list[i]) {
        //         Console.WriteLine("\t" + item.FullName);
        //     }
        // }

        return (file_info_list, topo_info_list);
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
            string inner = match.Groups["inner"].Value.Trim();
            if (inner == "") {
                return new FileInfo[0];
            }

            string[] inner_values = inner.Split(',');
            for (int i = 0; i < inner_values.Length; ++i) {
                inner_values[i] = inner_values[i].Trim();
                if (inner_values[i].Length >= 2) {
                    inner_values[i] = inner_values[i].Substring(1, inner_values[i].Length - 2);
                }
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
        throw new Exception();
    }
}