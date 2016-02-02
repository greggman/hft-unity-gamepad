/*
 * Copyright 2015, Gregg Tavares.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met:
 *
 *     * Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above
 * copyright notice, this list of conditions and the following disclaimer
 * in the documentation and/or other materials provided with the
 * distribution.
 *     * Neither the name of Gregg Tavares. nor the names of its
 * contributors may be used to endorse or promote products derived from
 * this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
using DeJson;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace HappyFunTimesEditor
{

    class HFTProcessWebFile : AssetPostprocessor
    {
        static private string HFT_SRC_PATH = "Assets/WebPlayerTemplates/HappyFunTimes/";
        static private string HFT_DST_PATH = Path.Combine(Path.Combine(Path.Combine("Assets", "HappyFunTimes"), "Resources"), "HappyFunTimesAutoGeneratedDoNotEdit");
        static private string HFT_DIR_PATH = Path.Combine(HFT_DST_PATH, "__dir__.txt");

        static private string DirPathFromAssetPath(string srcPath)
        {
            string filePath = srcPath.Substring(HFT_SRC_PATH.Length);
            string dstPath = Path.Combine(HFT_DST_PATH, filePath);
            return dstPath;
        }

        static private string DstPathFromAssetPath(string srcPath)
        {
            string dstPath = DirPathFromAssetPath(srcPath) + ".bytes";
            return dstPath;
        }

        static private void Error(string msg)
        {
            Debug.LogError(msg);
        }

        static private void CopyHFTFile(string srcPath) {
            string dstPath = DstPathFromAssetPath(srcPath);
            string dstDir  = DirPathFromAssetPath(srcPath);
            FileAttributes attr = File.GetAttributes(srcPath);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                try
                {
                    Directory.CreateDirectory(dstDir);
                }
                catch (System.Exception ex)
                {
                    Error("Error creating directory: " + dstDir + " -- " + ex);
                }
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(dstPath));
                    File.Copy(srcPath, dstPath, true);
//                    if (UnityEditor.EditorApplication.isPlaying)
//                    {
//                        SendToDB(srcPath);
//                    }
                }
                catch (System.Exception ex)
                {
                    Error("Error copying file: " + srcPath + " to" + dstPath + " -- " + ex);
                }
            }
        }

        static private void DeleteHFTFile(Dictionary<string, bool> directories, string srcPath) {
            string dstPath = DstPathFromAssetPath(srcPath);
            string dstDir  = DirPathFromAssetPath(srcPath);
            if (Directory.Exists(dstDir))
            {
                AddDir(directories, dstDir);
            }
            else
            {
                if (AssetDatabase.DeleteAsset(dstPath)) {
                    return;
                }
                try
                {
                    File.Delete(dstPath);
                    AddDir(directories, Path.GetDirectoryName(dstPath));
                }
                catch (System.Exception ex)
                {
                    Error("Error deleting file: " + dstPath + ": " + ex);
                }
            }
        }

        static private bool IsHFTFile(string path) {
            path = path.Replace("\\", "/");
            return path.StartsWith(HFT_SRC_PATH, System.StringComparison.OrdinalIgnoreCase);
        }

        static private string[] GetFileTreeList(string path, string basePath = null)
        {
            if (basePath == null)
            {
                basePath = path;
            }
            int baseLen = basePath.Length;
            List<string> outFiles = new List<string>();

            {
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    Match m = s_ignoreExtensionsRE.Match(file);
                    if (m.Success)
                    {
                        continue;
                    }
                    outFiles.Add(Path.Combine(path, file).Substring(baseLen + 1));
                }
            }

            {
                string[] directories = Directory.GetDirectories(path);
                foreach (string directory in directories)
                {
                    string[] subfiles = GetFileTreeList(Path.Combine(path, directory), basePath);
                    outFiles.AddRange(subfiles);
                }
            }

            string[] temp = new string[outFiles.Count];
            outFiles.CopyTo(temp);
            return temp;
        }

        static private void AddDir(Dictionary<string, bool> directories, string path)
        {
            directories[path] = true;
        }

        static private string[] GetSubDirs(string path)
        {
            List<string> all = new List<string>();

            if (Directory.Exists(path))
            {
                string[] dirs = Directory.GetDirectories(path);
                foreach (string dir in dirs)
                {
                    string[] subdirs = GetSubDirs(Path.Combine(path, dir));
                    all.AddRange(subdirs);
                }
            }

            string[] temp = new string[all.Count];
            all.CopyTo(temp);
            return temp;
        }

        static private void AddSubDirs(Dictionary<string, bool> directories)
        {
            List<string> dirs = new List<string>(directories.Keys);
            foreach (string path in dirs)
            {
                string[] subdirs = GetSubDirs(path);
                foreach (string subdir in subdirs)
                {
                    AddDir(directories, subdir);
                }
            }
        }

        static private void DeleteEmptyDirectories(Dictionary<string, bool> directories)
        {
            AddSubDirs(directories);
            List<string> dirs = new List<string>(directories.Keys);
            dirs.Sort();
            dirs.Reverse();

            foreach (string path in dirs)
            {
                if (Directory.Exists(path))
                {
                    string[] paths = {path};
                    string[] assets = AssetDatabase.FindAssets("t:Object", paths);
                    string[] subdirs = AssetDatabase.GetSubFolders(path);
                    if (assets.Length == 0 && subdirs.Length == 0)
                    {
                        if (AssetDatabase.DeleteAsset(path))
                        {
                            continue;
                        }

                        try
                        {
                            Directory.Delete(path);
                        }
                        catch (System.Exception ex)
                        {
                            Error("Error deleting directory: " + path + ": " + ex);
                        }
                    }
                }
            }
        }

        static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            Dictionary<string, bool> directories = new Dictionary<string, bool>();

            bool changes = false;
            foreach (var path in importedAssets)
            {
                if (IsHFTFile(path))
                {
                    changes = true;
                    CopyHFTFile(path);
                }
            }

            foreach (var path in deletedAssets)
            {
                if (IsHFTFile(path))
                {
                    changes = true;
                    DeleteHFTFile(directories, path);
                }
            }

            for (var ii = 0; ii < movedAssets.Length; ++ii)
            {
                string srcStr = movedFromAssetPaths[ii];
                string dstStr = movedAssets[ii];

                if (IsHFTFile(srcStr))
                {
                    changes = true;
                    DeleteHFTFile(directories, srcStr);
                }

                if (IsHFTFile(dstStr))
                {
                    changes = true;
                    CopyHFTFile(dstStr);
                }
            }

            if (changes)
            {
                string[] files = GetFileTreeList(Path.Combine(Path.GetDirectoryName(Application.dataPath), HFT_DST_PATH));
                files = files.Select(x => x.Substring(0, x.Length - 6)).ToArray();
                string s = Serializer.Serialize(files);
                File.WriteAllText(HFT_DIR_PATH, s);

                AssetDatabase.Refresh();

                DeleteEmptyDirectories(directories);
            }
        }

//        static void SendToDB(string srcPath)
//        {
//            string[] names = new string[]
//            {
//                "HappyFunTimes.HFTWebFileDB",
//                "HappyFunTimes.HFTWebFileDB, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
//                "HappyFunTimes.HFTWebFileDB, Assembly-CSharp-firstpass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
//                "HFTWebFileDB",
//            };
//            foreach (string name in names)
//            {
//                System.Type t = System.Type.GetType(name);
//                if (t != null)
//                {
//                    object o = UnityEngine.Object.FindObjectOfType(t);
//                    if (o != null)
//                    {
//                        MonoBehaviour mb = (MonoBehaviour)o;
//                        string path = srcPath.Substring(HFT_SRC_PATH.Length);
//                        byte[] content = System.IO.File.ReadAllBytes(srcPath);
//                        mb.gameObject.SendMessage("AddFilePair", new KeyValuePair<string, byte[]>(path, content));
//                    }
//                }
//            }
//
////            HFTWebFileDB.GetInstance().AddFile("foo", new bytes[10]());
//        }

        private static Regex s_ignoreExtensionsRE = new Regex(@"(\.meta|\.DS_Store|__dir__\.txt)$");
    }

}  // namespace HappyFunTimesEditor


