using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

public class LidarReceiver : MonoBehaviour {
    public class Measurement {
        public int distance = 0;
        public int ss = 0; // signal strength
        public bool error = true;
		public bool strengthWarning = true;
		public int errorcode = 0;
		public int parseDistance(int input) {
			error = (input & 0x8000) > 0;
			strengthWarning = (input & 0x4000) > 0;
			if (error) {
				errorcode = input & 0xFF;
				distance = 0;
			} else {
				distance = input;
			}
			if ((input & 0x4000) > 0) {
				strengthWarning = true;
			}
			// Debug.Log(string.Format("d: {0:x4} | {1}", input, distance));
			return distance;
		}
    }

    public string comPort;
	public Measurement[] measurements;

    private SerialPort serial;
	private System.Diagnostics.Process inputProcess = new System.Diagnostics.Process();

	private List<byte> receivingBuffer = new List<byte>();

    private uint readBytes = 0;
	private uint discardedBytes = 0;
	private uint parsedBytes = 0;
	private uint parsedChunks = 0;

	public enum Modes {
		SERIAL_PORT,
		SERIAL_PORT_LINUX,
		FILE_SIMULATION_LINUX,
	};
	public Modes usageMode = Modes.SERIAL_PORT;

    // Use this for initialization
    void Start() {
		measurements = new Measurement[360];
		for (int i = 0; i < measurements.Length; i++) {
			measurements[i] = new Measurement();
		}

		string[] ports = SerialPort.GetPortNames();

		Debug.Log(string.Join(", ", ports));

		if (usageMode == Modes.SERIAL_PORT) {
			serial = new SerialPort(comPort);
			serial.BaudRate = 115200;
			serial.Parity = Parity.None;
			serial.DataBits = 8;
			serial.StopBits = StopBits.One;

			serial.DataReceived += SerialDataReceivedHandler;
			serial.Open();
		} else {
			if (usageMode == Modes.SERIAL_PORT_LINUX) {
				inputProcess.StartInfo.FileName = "/usr/bin/cu";
				inputProcess.StartInfo.Arguments = string.Format("-l {0} -s 115200", comPort);
			} else if (usageMode == Modes.FILE_SIMULATION_LINUX) {
				inputProcess.StartInfo.FileName = "/bin/cat";
				inputProcess.StartInfo.Arguments = "simulation-raw.data";
			} else {
				throw new System.Exception("Unsupported usage Mode");
			}

			inputProcess.StartInfo.CreateNoWindow = false;
			inputProcess.StartInfo.UseShellExecute = false;
			inputProcess.StartInfo.RedirectStandardError = true;
			inputProcess.StartInfo.RedirectStandardInput = true;
			inputProcess.StartInfo.RedirectStandardOutput = true;

			inputProcess.Exited += ProcessExited;
			inputProcess.Start();
		}
    }

    private void ProcessExited(object sender, System.EventArgs e) {
		Debug.Log(string.Format("exited: {0} / {1}", sender, e));
		
		Debug.Log(string.Format("err: {0}", inputProcess.StandardError.ReadToEnd()));
		Debug.Log(string.Format("out: {0}", inputProcess.StandardOutput.ReadToEnd()));
		Debug.Log(string.Format("exit code: {0}", inputProcess.ExitCode));
    }

    private void ProcessOutputReceived(object sender, System.Diagnostics.DataReceivedEventArgs e) {
		Debug.Log(string.Format("Data: {0}", System.BitConverter.ToString(Encoding.ASCII.GetBytes(e.Data))));
		byte[] bla = Encoding.ASCII.GetBytes(e.Data);
		foreach(byte b in bla) {
			receivingBuffer.Add(b);
		}
    }

    void SerialDataReceivedHandler(object sender, SerialDataReceivedEventArgs e) {
		SerialPort source = (SerialPort) sender;
		Debug.Log(string.Format("Data: {0}", source.ReadExisting()));
	}

	void TryParsing() {
		Profiler.BeginSample("cc-parsing");
		
		if (receivingBuffer.Count < 22) {
			Profiler.EndSample();
			return; // should have two full segments buffered, just in case.
		}

		if (!receivingBuffer[0].Equals(0xFA)) {
			// Debug.Log(string.Format("skipping {0:X}", receivingBuffer[0]));
			receivingBuffer.RemoveAt(0);
			discardedBytes++;
			Profiler.EndSample();
			return;
		}

		int index = receivingBuffer[1] - 0xA0;
		if (index < 0 || index > 90) {
			receivingBuffer.RemoveAt(0);
			discardedBytes++;
			Profiler.EndSample();
			return; // index must be within those two bounds.
		}
		index = index * 4;
		byte[] data = receivingBuffer.ToArray();

		int speed = data[3] << 8 | data[2];
		for (int i = 0; i < 4; i++) {
			Measurement m = measurements[index + i];
			int offset = i * 4;
			m.parseDistance(data[5 + offset] << 8 | data[4 + offset]);
			m.ss = data[7 + offset] << 8 | data[6 + offset];
		}

		int checksum = data[21] << 8 | data[20];
		receivingBuffer.RemoveRange(0, 22); // 22 byte message parsed.
		parsedBytes += 22;
		parsedChunks++;

		Profiler.EndSample();
	}

    // Update is called once per frame
    void Update() {
		Profiler.BeginSample("cc-receive");
		int b;
		int counter = 0;
		while (((b = inputProcess.StandardOutput.BaseStream.ReadByte()) != -1) && (++counter < 220)) {
			readBytes++;
			string text = string.Format("{0:x2}", b);
			byte currentByte = System.Convert.ToByte(b);
			receivingBuffer.Add((byte) b);
		}
		Profiler.EndSample();
		while (receivingBuffer.Count > 22) {
			TryParsing();
		}
    }
}
