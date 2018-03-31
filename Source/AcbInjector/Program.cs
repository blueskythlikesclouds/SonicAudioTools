using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SonicAudioLib.CriMw;
using SonicAudioLib.Archives;
using System.IO;

using HedgeEdit.UI;

namespace AcbInjector
{
    class Program
    {
        static List<FileInfo> Junk = new List<FileInfo>();

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();

            if (args.Length < 1)
            {
                MessageBox.Show("This program can inject audio files into ACB files without the need of repacking their AWBs.\n\nTo start, drag and drop an ACB file to the executable.", "ACB Injector", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string sourceFileName = null;
                string sourceAudioFileName = null;
                int waveformId = -1;
                string destinationFileName = null;

                foreach (var arg in args)
                {
                    if (int.TryParse(arg, out int ret) && waveformId < 0)
                    {
                        waveformId = ret;
                    }

                    else if (sourceFileName == null)
                    {
                        sourceFileName = arg;
                    }

                    else if (sourceAudioFileName == null)
                    {
                        sourceAudioFileName = arg;
                    }

                    else if (destinationFileName == null)
                    {
                        destinationFileName = arg;
                    }
                }

                CriTable acbFile = new CriTable();
                acbFile.Load(sourceFileName);

                CriTable waveformTable = new CriTable();
                waveformTable.Load(acbFile.Rows[0].GetValue<byte[]>("WaveformTable"));

                var waveforms = waveformTable.Rows.Where(x => x.GetValue<byte>("Streaming") == 0).ToList();
                var streamedWaveforms = waveformTable.Rows.Except(waveforms).ToList();

                if (streamedWaveforms.Count < 1)
                {
                    throw new InvalidDataException("This ACB file has no streamed waveforms, aka an AWB file.");
                }

                if (waveformId < 0)
                {
                    while (true)
                    {
                        using (TxtBxDialog textBoxDialog = new TxtBxDialog(
                            streamedWaveforms.Select(x => x.GetValue<ushort>("Id")).OrderBy(x => x).Select(x => x.ToString()).ToArray()))
                        {
                            if (textBoxDialog.ShowDialog() == DialogResult.OK)
                            {
                                if (!int.TryParse(textBoxDialog.Result, out waveformId) || !streamedWaveforms.Any(x => x.GetValue<ushort>("Id") == waveformId))
                                {
                                    MessageBox.Show("Invalid waveform id.", "ACB Injector", MessageBoxButtons.OK);
                                }

                                else
                                {
                                    break;
                                }
                            }

                            else
                            {
                                return;
                            }
                        }
                    }
                }

                CriRow waveformToInject = streamedWaveforms.FirstOrDefault(
                    x => x.GetValue<ushort>("Id") == waveformId);

                int newWaveformId = (waveforms.Count > 0 ?
                    waveforms.Max(x => x.GetValue<ushort>("Id")) : -1) + 1;

                waveformToInject["Id"] = (ushort)newWaveformId;
                waveformToInject["Streaming"] = (byte)0;

                if (string.IsNullOrEmpty(sourceAudioFileName))
                {
                    using (OpenFileDialog openFileDialog = new OpenFileDialog
                    {
                        Title = "Select the audio file that you are going to inject",
                        InitialDirectory = Path.GetDirectoryName(sourceFileName),
                        Filter = "All Files|*.*",
                    })
                    {
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            sourceAudioFileName = openFileDialog.FileName;
                        }

                        else
                        {
                            return;
                        }
                    }
                }

                CriAfs2Archive archive = new CriAfs2Archive();

                if (acbFile.Rows[0]["AwbFile"] is byte[] archiveData && archiveData.Length > 0)
                {
                    archive.Load(archiveData);

                    // Proof that SonicAudioLib needs a rewrite.
                    // I hate this...
                    foreach (var entry in archive)
                    {
                        var filePath = new FileInfo(
                            Path.GetTempFileName());

                        File.WriteAllBytes(filePath.FullName,
                            archiveData.Skip((int)entry.Position).Take((int)entry.Length).ToArray());

                        entry.FilePath = filePath;
                        Junk.Add(entry.FilePath);
                    }
                }

                archive.Add(new CriAfs2Entry
                {
                    Id = (uint)newWaveformId,
                    FilePath = new FileInfo(sourceAudioFileName)
                });

                acbFile.Rows[0]["AwbFile"] = archive.Save();

                acbFile.WriterSettings =
                    waveformTable.WriterSettings =
                        CriTableWriterSettings.Adx2Settings;

                acbFile.Rows[0]["WaveformTable"] = waveformTable.Save();

                if (string.IsNullOrEmpty(destinationFileName))
                {
                    if (args.Length < 2)
                    {
                        using (SaveFileDialog saveFileDialog = new SaveFileDialog
                        {
                            Title = "Save ACB file",
                            InitialDirectory = Path.GetDirectoryName(sourceFileName),
                            FileName = Path.GetFileName(sourceFileName),
                            Filter = "ACB Files|*.acb",
                        })
                        {
                            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                destinationFileName = saveFileDialog.FileName;
                            }

                            else
                            {
                                return;
                            }
                        }
                    }

                    else
                    {
                        destinationFileName = sourceFileName;
                    }
                }

                acbFile.Save(destinationFileName);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ACB Injector", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            foreach (var junk in Junk)
            {
                junk.Delete();
            }
        }
    }
}
