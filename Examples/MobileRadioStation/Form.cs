using System;
using System.Windows.Forms;
using InTheHand.Net.Sockets;
using InTheHand.Windows.Forms;
using InTheHand.Net;
using System.IO;
using Brecham.Obex;
using InTheHand.Net.Bluetooth;
using NAudio;
using NAudio.Wave.SampleProviders;
using NAudio.Wave.Compression;
using NAudio.Wave;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using NAudio.Lame;

namespace blue
{
    public partial class Form1 : Form
    {
        public WaveIn waveInStream;
        public WaveFileWriter writer;
        public  BluetoothDeviceInfo deviceInfo = null;
        public int choice;
        public int filter;

        public Form1()
        {
            InitializeComponent();
            filter = 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            choice = 0;
            ChooseDevice();
        }

        public void ChooseDevice()
        {
            var dialog = new SelectBluetoothDeviceDialog();
            dialog.ShowAuthenticated = true;
            dialog.ShowRemembered = true;
            dialog.ShowUnknown = true;
            BluetoothClient client = new BluetoothClient();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                deviceInfo = dialog.SelectedDevice;
                deviceInfo.SetServiceState(BluetoothService.ObexObjectPush, true);
             //   var ep = new BluetoothEndPoint(deviceInfo.DeviceAddress, BluetoothService.ObexObjectPush);
              //  client.Connect(ep);
                if (deviceInfo.Connected)
                {
                    label4.Visible = true;
                    label4.Text = "Połączono z " + deviceInfo.DeviceName;
                }
                else
                {
                }
            }
            else
            {
                this.Close();
            };
            }

        private void button3_Click(object sender, EventArgs e)
        {
            choice = 1;
            button1.Visible = true;
            button2.Visible = true;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            label3.Visible = true;
            label5.Visible = true;
            label3.Text = "Nagrywanie rozpoczęte";
            timer1.Start();
            string outputFilename = @"out.wav";

            waveInStream = new WaveIn();
            WaveFormat format = new WaveFormat(44100,16,2);
            waveInStream.WaveFormat = format;
            writer = new WaveFileWriter(outputFilename, format);

            waveInStream.DataAvailable += new EventHandler<WaveInEventArgs>(waveInStream_DataAvailable);
            waveInStream.StartRecording();
        }

        void waveInStream_DataAvailable(object sender, WaveInEventArgs e)
        {
            writer.Write(e.Buffer, 0, e.BytesRecorded);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            label3.Text = "Nagrywanie zakończone";
            label3.Visible = true;
            waveInStream.StopRecording();
            waveInStream.Dispose();
            waveInStream = null;
            writer.Dispose();
            writer.Close();
            writer = null;
            WavToMp3();
        }

   public void WavToMp3()
        {
            string inputFileName = @"out.wav";
            string outputFileName = @"out.mp3";
            using (var reader = new WaveFileReader(inputFileName))
            using (var writer = new LameMP3FileWriter(outputFileName, reader.WaveFormat,128))
                reader.CopyTo(writer);
            File.Delete("out.wav");
            AddCTCSS();
            SendFile();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            choice = 0;
            AddCTCSS();
            SendFile();
        }

        public List<float> ReadFromMP3()
        {
             string filePath;
             if (choice == 0)
             {
                 filePath = ChooseFile();
             }else
             {
                 filePath = @"out.mp3";
             }

             List<float> allSamples = new List<float>();
             float[] samples = new float[256];

             AudioFileReader sampleProvider = new NAudio.Wave.AudioFileReader(filePath);

             int channels = sampleProvider.WaveFormat.Channels;


             while (sampleProvider.Read(samples, 0, samples.Length) > 0)
              {
                  if (channels == 2)
                  {
                      for (int i = 0; i < samples.Length; i = i + 2)
                      {
                          allSamples.Add((samples[i]+samples[i+1])/2);
                      }
                  } else
                  {
                      for (int i = 0; i < samples.Length; i++)
                      {
                          allSamples.Add(samples[i]);
                      }
                  }
              }

            return allSamples;
                        
        }

        public string ChooseFile()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "mp3 files (*.txt)|*.mp3";
            string filePath;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                filePath = dialog.FileName;
                return filePath;
            }
            return null;
        }


        public float[] Convolution()
        {

            List<float> samples = ReadFromMP3();
            List<float> FIR = ReadFilter();


            int samplesLength = samples.Count;
            int firLength = FIR.Count;
            int k = samplesLength + firLength - 1;
            float[] afterConv = new float[k];

            for (int i=0;i<k;i++)
            {
                for (int j=0;j<firLength;j++)
                {
                    if( i-j >= 0 && i-j < samplesLength)
                    {
                        afterConv[i] += samples[i - j] * FIR[j];
                    }
                }
            }
            return afterConv;
        }

        public List<float> ReadFilter()
        {
            List<float> filterFIR = new List<float>();
            string line;
            StreamReader sr;
            if (filter == 0)
            {
                sr = new StreamReader("3kHz.txt");
            }
            else
            {
                sr = new StreamReader("2kHz.txt");
            }


            using (sr)
            {
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Replace(".", ",");
                    float fir = float.Parse(line);
                    filterFIR.Add(fir);
                }
            }
            return filterFIR;
        }

        public List<float> Sampling()
        {
            float[] samples = Convolution();
            const int M = 6;
            List<float> allSamples = new List<float>();

            for (int i=0; i < samples.Length; i=i+M)
            {
                allSamples.Add(samples[i]);
            }

            return allSamples;
        }

        public void AddCTCSS()
        {
            List<float> allSamps = Sampling();
            List<float> allSamples = new List<float>();
            List<float> sinCTCSS = new List<float>();

            double Fs = 7350.0;
            double dt = 1 / Fs;
            double T = allSamps.Count()/Fs;
            double F = 107.2;
            double A = 0.119;

            for (double i=0; i< T; i=i+dt)
            {

                sinCTCSS.Add((float)(A*Math.Sin(2*Math.PI*F*i)));
            }

            int j = 0;
            for (int i = 0; i < allSamps.Count; i++)
            {
                allSamples.Add(allSamps[i]+sinCTCSS[i]);
            }

            TextWriter tw = new StreamWriter("FileToSend.txt");
            foreach (float a in allSamples)
            {
                tw.WriteLine(a.ToString().Replace(",", "."));
            }

            tw.Close();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            filter = 1;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            filter = 0;
        }
        int count = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            label5.Text = "Czas : "+count.ToString()+" sek.";
            count++;
        }

        public void SendFile()
        {
            string fileToSend = @"FileToSend.txt";
            System.Uri obexUri = new Uri("obex://" + deviceInfo.DeviceAddress.ToString() + "/" + fileToSend);
            var request = new ObexWebRequest(obexUri);
            request.ReadFile(fileToSend);
            if (deviceInfo.Connected == true)
            {
                var response = request.GetResponse() as ObexWebResponse;
                MessageBox.Show(response.StatusCode.ToString());
                response.Close();
            }
            else
            {
                MessageBox.Show("Wybrane urządzenie nie jest połączone!", "Uwaga!");
            }
        }
    }
}
