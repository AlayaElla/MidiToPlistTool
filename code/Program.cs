using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Multimedia.Midi;


namespace MidiToPlist
{
    class Program
    {

        static Dictionary<string, string> compatable = new Dictionary<string, string>();
        static void Main(string[] args)
        {
            string filepath = "D:\\DoGame\\MidiToPlistTool\\bin\\Release\\[Air]夢語り 指弹.mid";
            string nowpath = AppDomain.CurrentDomain.BaseDirectory;
            compatable = ReadCompatable(nowpath + "compatable.txt");

            //MidiFileReader _smr = new MidiFileReader(filepath);
            //ArrayList _notes = GetNote(_smr);

            if (args.Length >= 1)
            {
                foreach (string f in args)
                {
                    filepath = f;

                    string path = Path.GetDirectoryName(filepath);
                    string filename = Path.GetFileName(filepath);
                    string Extension = Path.GetExtension(filepath);

                    if (Extension == ".midi" || Extension == ".mid")
                    {
                        MidiFileReader smr = new MidiFileReader(filepath);
                        ArrayList notes = GetNote(smr);
                        CreateFille(filename + ".plist", path, notes);
                    }

                    else
                    {
                        Console.WriteLine("这个文件是:" + filename);
                        Console.WriteLine("请放入midi文件。。");
                        Console.ReadLine();
                    }
                }
            }
        }

        static ArrayList GetNote(MidiFileReader r)
        {
            ArrayList notes = new ArrayList();
            string[] noteinfo;

            if (r.tracks.Length > 0)
            {
                Track _t = r.tracks[0];
                ArrayList events = _t.GetMidiEvents();

                int absoluteTime = 0;
                int lastTime = 0;

                foreach (MidiEvent e in events)
                {
                    absoluteTime += e.Ticks;

                    if (ChannelMessage.IsChannelMessage(e.Message.Status))
                    {
                        ChannelMessage msg = (ChannelMessage)e.Message;
                        if (msg.Command == ChannelCommand.NoteOn)
                        {
                            if (compatable.ContainsKey(msg.Data1.ToString()))
                            {
                                if (notes.Count >= 1 && absoluteTime == lastTime)
                                {
                                    noteinfo = (string[])notes[notes.Count - 1];
                                    noteinfo[0] = noteinfo[0] + "," + compatable[msg.Data1.ToString()];

                                    notes[notes.Count - 1] = noteinfo;
                                }
                                else
                                {
                                    noteinfo = new string[2];
                                    noteinfo[0] = compatable[msg.Data1.ToString()];
                                    noteinfo[1] = absoluteTime.ToString();
                                    notes.Add(noteinfo);
                                }
                            }
                            else
                            {
                                if (notes.Count >= 1 && absoluteTime == lastTime)
                                {
                                    noteinfo = (string[])notes[notes.Count - 1];
                                    noteinfo[0] = noteinfo[0] + "," + "没解析到:" + msg.Data1.ToString();

                                    notes[notes.Count - 1] = noteinfo;
                                }
                                else
                                {
                                    noteinfo = new string[2];
                                    noteinfo[0] = "没解析到:" + msg.Data1.ToString();
                                    noteinfo[1] = absoluteTime.ToString();
                                    notes.Add(noteinfo);
                                }
                            }
                            lastTime = absoluteTime;
                        }
                    }
                }
            }
            return notes;
        }

        static void CreateFille(string filename,string path,ArrayList notes)
        {
            if (!File.Exists(filename))
            {
                FileStream fs1 = new FileStream(filename, FileMode.Create, FileAccess.Write);//创建写入文件 
                StreamWriter sw = new StreamWriter(fs1);

                //开始写入值
                sw.WriteLine("<!--  filename:" + filename + "  -->");
                sw.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\"/>\r\n<plist version=\"1.0\">\r\n\t<dict>\r\n");

                sw.Write("\t\t<key>num</key>\r\n" + "\t\t<string>" + notes.Count.ToString() + "</string>\r\n");

                for (int i = 0; i < notes.Count; i++)
                {
                    string[] noteinfo = (string[])notes[i];
                    sw.Write("\t\t<key>" + (i + 1) + "</key>\r\n" + "\t\t<string>" + noteinfo[0].ToString() + "</string>\r\n");
                }

                sw.Write("\t</dict>\r\n</plist>");
                sw.Close();
                fs1.Close();
            }
            else
            {
                FileStream fs = new FileStream(filename, FileMode.Truncate, FileAccess.Write);
                StreamWriter sr = new StreamWriter(fs);
                
                //开始写入值
                sr.WriteLine("<!--  filename:" + filename + "  -->");
                sr.Write("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\"/>\r\n<plist version=\"1.0\">\r\n\t<dict>\r\n");

                sr.Write("\t\t<key>num</key>\r\n" + "\t\t<string>" + notes.Count.ToString() + "</string>\r\n");

                for (int i = 0; i < notes.Count; i++)
                {
                    string[] noteinfo = (string[])notes[i];
                    sr.Write("\t\t<key>" + (i + 1) + "</key>\r\n" + "\t\t<string>" + noteinfo[0].ToString() + "</string>\r\n");
                }

                sr.Write("\t</dict>\r\n</plist>");
                sr.Close();
                fs.Close();
            }
        }

        static Dictionary<string, string> ReadCompatable(string filepath)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            StreamReader sr = null;
            try
            {
                sr = File.OpenText(filepath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            string line;
            //逐行读取
            while ((line = sr.ReadLine()) != null)
            {
                string[] str = new string[2];
                str = System.Text.RegularExpressions.Regex.Split(line, "\t");
                dic.Add(str[0], str[1]);
            }
            sr.Close();
            sr.Dispose();

            return dic;
        }
    }
}
