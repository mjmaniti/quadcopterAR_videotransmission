using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading;
using System.Text;
using System;
using System.Net.Sockets;

[Serializable]
public class Position
{
	public float x;
	public float y;
	public float z;
}

[Serializable]
public struct Orientation
{
	public float w;
	public float x;
	public float y;
	public float z;
}

[Serializable]
public class PoseJSON
{
	public Position position;
	public Orientation orientation;
}

public class DroneConnection : MonoBehaviour {

	public string remoteIP;
	public bool isSimulation;
	public GameObject droneObject;

	private Thread clientReceiveThread;
	private TcpClient socketConnection;

	private Vector3 current_position = new Vector3(0, 0, 0);
	private Vector3 current_orientation = new Vector3(0, 0, 0);
	private Vector3 target_displacement = new Vector3(0, 0, 0);
	private Vector3 target_position = new Vector3(0, 0, 0);
	private Vector3 target_orientation = new Vector3(0, 0, 0);

	// Use this for initialization
	void Start () {
		ConnectToTcpServer();
	}
	
	// Update is called once per frame
	void Update () {
		droneObject.transform.position = current_position;
		droneObject.transform.eulerAngles = current_orientation;
	}

	private void ConnectToTcpServer()
	{
		try
		{
			clientReceiveThread = new Thread(new ThreadStart(ListenForData));
			clientReceiveThread.IsBackground = true;
			clientReceiveThread.Start();
		}
		catch (Exception e)
		{
			Debug.Log("On client connect exception " + e);
		}
	}

	private void ListenForData()
	{
		try
		{
			socketConnection = new TcpClient(remoteIP, 13500);
			
			if (socketConnection.Connected)
				Debug.Log("TCP Server connected.");
			while (true)
			{
				// Get a stream object for reading              
				using (NetworkStream stream = socketConnection.GetStream())
				{
					Byte[] length_bytes = new Byte[16];


					// Read incomming stream into byte arrary.                  
					while (true)
					{
						try
						{
							stream.Read(length_bytes, 0, length_bytes.Length);
							int length = Convert.ToInt32(Encoding.ASCII.GetString(length_bytes));


							var bytes = new byte[length];
							stream.Read(bytes, 0, bytes.Length);

							Byte[] send_msg = Encoding.UTF8.GetBytes("RECV");
							stream.Write(send_msg, 0, send_msg.Length);

							string json_str = Encoding.UTF8.GetString(bytes);
							// Debug.Log(json_str);
							PoseJSON p = JsonUtility.FromJson<PoseJSON>(json_str);
							// Debug.Log(p.position.x);


							// string[] pose_str = serverMessage.Split(',');
							float x = p.position.x;
							float y = p.position.y;
							float z = p.position.z;
							Vector3 ros_current_position = new Vector3(x, y, z);

							// Coord conversion from ROS to Unity should have been done here.
							if (isSimulation)
							{
								// Gazebo
								current_position.x = ros_current_position.x;
								current_position.y = ros_current_position.z;
								current_position.z = ros_current_position.y;
							}
							else
							{
								// [TEST OK] 3DR
								current_position.x = -ros_current_position.y;
								current_position.y = ros_current_position.z;
								current_position.z = ros_current_position.x;

								// [TEST OK] RealSense
								current_position.x = ros_current_position.x;
								current_position.y = ros_current_position.y;
								current_position.z = -ros_current_position.z;
							}

							Vector3 ros_current_euler_orientation = (new Quaternion(p.orientation.x, p.orientation.y, p.orientation.z, p.orientation.w)).eulerAngles;

							// Vector3 ros_current_euler_orientation = new Vector3();
							// ros_current_euler_orientation.x = p.position.x;
							// ros_current_euler_orientation.y = p.position.x);
							// ros_current_euler_orientation.z = p.position.x);

							// From ROS_PoseStamped.cs euler.y, -euler.z  , -euler.x
							// x = y, y = -z, z = -x [TEST NOT OK]

							// MY TRANSFORMATION x=-x, y=-z, z=-y (Right-handed to Left-handed)
							if (isSimulation)
							{
								// [TEST OK] on Simulation
								current_orientation.x = -ros_current_euler_orientation.x;
								current_orientation.y = -ros_current_euler_orientation.z;
								current_orientation.z = -ros_current_euler_orientation.y;
							}
							else
							{
								// [TEST OK] on 3DR
								current_orientation.x = ros_current_euler_orientation.y;
								current_orientation.y = -ros_current_euler_orientation.z;
								current_orientation.z = -ros_current_euler_orientation.x;

								// [TEST OK] on RealSense
								current_orientation.x = -ros_current_euler_orientation.x;
								current_orientation.y = -ros_current_euler_orientation.y;
								current_orientation.z = -(-ros_current_euler_orientation.z);
							}

							// Debug.Log("current_position " + current_position);
						}
						catch (Exception e)
						{
							Debug.Log(e);
						}
						
						
					}
				}
			}
		}

		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}

	void OnDestroy()
	{
		clientReceiveThread.Abort();
		Debug.Log("OnDestroy1");
	}
}
