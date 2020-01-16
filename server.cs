using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;

public class server : MonoBehaviour {

	// private TcpListener tcpListener;
	private Thread[] tlts;
	// private TcpClient connectedTcpClient;
	// private Byte[] length_bytes;
	private TcpClient[] TcpClients;
	private Byte[] image_bytes;
	private Texture2D plane_texture;
	private bool new_frame;
	private Queue<byte[]> frame_queue;

	[SerializeField]
	private int port_number;

	[SerializeField]
	private int buff_size;
	
	[SerializeField]
	private int thread_number;

	// Use this for initialization
	void Start()
	{
		if(port_number == 0)
		{
			port_number = 5001;
		}

		if(buff_size == 0)
		{
			buff_size = 1024;
		}

		if (thread_number == 0)
		{
			thread_number = 1;
		}



		tlts = new Thread[thread_number];

		for(int i=0; i< thread_number; i++)
		{
			tlts[i] = new Thread(new ParameterizedThreadStart(ListenForIncommingRequests));
			tlts[i].IsBackground = true;
			tlts[i].Start(port_number + i);
		}
		/*
		tlt_1 = new Thread(new ParameterizedThreadStart(ListenForIncommingRequests));
		tlt_1.IsBackground = true;
		tlt_1.Start(port_number);

		tlt_2 = new Thread(new ParameterizedThreadStart(ListenForIncommingRequests));
		tlt_2.IsBackground = true;
		tlt_2.Start(port_number + 1);
		*/



		plane_texture = new Texture2D(2560, 720);
		frame_queue = new Queue<byte[]>();
		
	}

	// Update is called once per frame
	void Update()
	{
		// if (image_bytes != null && image_bytes.Length > 0 && frame_queue.Peek() != null)
		if(frame_queue.Count > 0)
		{
			try
			{
				// Debug.Log("Converting texture");
				// plane_texture.LoadRawTextureData(image_bytes);
				// if (image_bytes[0] == 255 && image_bytes[1] == 216 && image_bytes[2] == 255 && image_bytes[3] == 224 && image_bytes[4] == 0 && image_bytes[5] == 16 && image_bytes[6] == 74 && image_bytes[7] == 70 && image_bytes[8] == 73 && image_bytes[9] == 70)
				// {
				plane_texture.LoadImage(frame_queue.Dequeue());
				//Material Right_Eye_Mat = GameObject.Find("Plane_Right_Eye").GetComponent<Renderer>().material;
				Material Right_Eye_Mat = GameObject.Find("Right_Curved_Plane").GetComponent<MeshRenderer>().material;
				//Material Left_Eye_Mat = GameObject.Find("Plane_Left_Eye").GetComponent<Renderer>().material;
				Material Left_Eye_Mat = GameObject.Find("Left_Curved_Plane").GetComponent<MeshRenderer>().material;

				Right_Eye_Mat.mainTexture = plane_texture;
				//Right_Eye_Mat.SetTexture("_EmissionMap", plane_texture);
				//Right_Eye_Mat.SetTextureOffset("_EmissionMap", new Vector2(.5f, 0));
				//Right_Eye_Mat.SetTextureScale("_EmissionMap", new Vector2(.5f, 1.0f));
				Right_Eye_Mat.mainTextureOffset = new Vector2(.5f, 0);
				Right_Eye_Mat.mainTextureScale = new Vector2(.5f, 1.0f);
				Left_Eye_Mat.mainTexture = plane_texture;
				Left_Eye_Mat.mainTextureScale = new Vector2(.5f, 1.0f);
				//Debug.Log("OK");
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
			}
		}
	}

	private void ListenForIncommingRequests(object port_num_obj)
	{
		try
		{
			TcpListener tcpListener;
			// Create listener on localhost port 8052. 			
			tcpListener = new TcpListener(IPAddress.Parse("0.0.0.0"), (int)port_num_obj);
			tcpListener.Start();
			Debug.Log("Server is listening on port " + ((int)port_num_obj).ToString());


			while (true)
			{
				TcpClient connectedTcpClient;
				connectedTcpClient = tcpListener.AcceptTcpClient();
				// TcpClients[TcpClients.Length] = connectedTcpClient;
				Debug.Log("Connected from " + ((IPEndPoint)connectedTcpClient.Client.RemoteEndPoint).Address.ToString());
				// Debug.Log("Connected on port " + ((int)port_num_obj).ToString());

				while (connectedTcpClient.Connected)
				{
					NetworkStream stream = connectedTcpClient.GetStream();

					Byte[] length_bytes;

					length_bytes = new Byte[16];
					stream.Read(length_bytes, 0, 16);
					int length = Convert.ToInt32(Encoding.ASCII.GetString(length_bytes));
					// Debug.Log("Receiving data of length = " + length.ToString());

					Byte[] full_buff = new Byte[length];
					int current_length = 0;
					int chunk_length;
					Byte[] buff = new Byte[buff_size];

					while (stream.CanRead && current_length < length)
					{
						while (stream.DataAvailable && (chunk_length = stream.Read(buff, 0, buff_size)) != 0)
						{
							for (int i = 0; i < chunk_length; i++)
							{
								full_buff[i + current_length] = buff[i];
							}
							current_length += chunk_length;
						}
					}

					// JPEG Format Check
					if (full_buff[0] == 255 && full_buff[1] == 216) {
						// image_bytes = full_buff;
						Debug.Log("ENQUEUE");
						frame_queue.Enqueue(full_buff);
					}

					// Request Next Frame
					byte[] msg = Encoding.UTF8.GetBytes("NEXTFRAME");
					stream.Write(msg, 0, msg.Length);
					// Debug.Log("Request next frame on port " + ((int)port_num_obj).ToString());
				}
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("SocketException " + socketException.ToString());
		}
	}

	void OnApplicationQuit()
	{
		for(int i = 0; i < tlts.Length; i++)
		{
			tlts[i].Abort();
		}
		/*
		if (TcpClients[0] != null && TcpClients[0].Connected)
			TcpClients[0].Close();
		if (TcpClients[1] != null && TcpClients[1].Connected)
			TcpClients[1].Close();
			*/
	}
}
