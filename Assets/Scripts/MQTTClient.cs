using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

public class MQTTClient : MonoBehaviour {
	public string hostname;
	public int port;
	public string topic;

	public Vector3 direction;
	private MqttClient client;

	// Use this for initialization
	void Start () {
		client = new MqttClient(hostname, port, false, null, null, MqttSslProtocols.None);
		client.MqttMsgPublishReceived += client_MqttReceived;

		client.Connect("unity");
		client.Subscribe(new string[] {topic}, new byte[] {MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE});
	}

	void client_MqttReceived(object sender, MqttMsgPublishEventArgs e) {
		string msg = Encoding.UTF8.GetString(e.Message);
		string[] bits = msg.Split(',');
		direction.x = float.Parse(bits[0], CultureInfo.InvariantCulture);
		direction.y = float.Parse(bits[1], CultureInfo.InvariantCulture);
		direction.z = float.Parse(bits[2], CultureInfo.InvariantCulture);
	}

	// Update is called once per frame
	void Update () {
		
	}
}
