using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using FirstGameNiteJam;

[StructLayout(LayoutKind.Sequential)]
public struct c_flat_string_vec
{
    public IntPtr chars_ptr;
    public UInt64 chars_len;
    public UInt64 chars_cap;
    public IntPtr lens_ptr;
    public UInt64 lens_len;
    public UInt64 lens_cap;
}

public class ControlpadsLibrary
{
    [DllImport("c_sharp_controlpads.dll")]
    public static extern int free_c_flat_string_vec(c_flat_string_vec c_flat);
    [DllImport("c_sharp_controlpads.dll")]
    public static extern int clients_changed(ref bool did_change);
    [DllImport("c_sharp_controlpads.dll")]
    public static extern int get_client_handles(ref c_flat_string_vec client_handles);
    [DllImport("c_sharp_controlpads.dll")]
    public static extern int send_message([MarshalAs(UnmanagedType.LPStr)] string client, [MarshalAs(UnmanagedType.LPStr)] string msg);
    [DllImport("c_sharp_controlpads.dll")]
    public static extern int get_messages([MarshalAs(UnmanagedType.LPStr)] string client, ref c_flat_string_vec messages);
}



public class controlpads_glue : MonoBehaviour
{
    List<string> clientHandles = new List<string>();

    // Update is called once per frame
    void Update()
    {
        if (ClientsChanged())
        {
            clientHandles = GetClientHandles();
            Debug.Log(string.Join(", ", clientHandles));
        }
        foreach (string client in clientHandles)
        {
            foreach (string msg in GetMessages(client))
            {
                GameManager.Instance.HandleMessage(client, msg);
                string s = string.Format("{0} said: {1}", client, msg);
                Debug.Log(s);
                SendMessage(client, s);
            }
        }
    }

    // ---- Library Glue Functions ----
    public bool ClientsChanged()
    {
        bool b = false;
        int result = ControlpadsLibrary.clients_changed(ref b);
        if (result != 0)
        {
            Debug.Log(string.Format("Controlpads Error (clients_changed): {0}", result));
        }
        return b;
    }

    public List<string> GetClientHandles()
    {
        c_flat_string_vec c_flat_handles = new c_flat_string_vec();
        int result = ControlpadsLibrary.get_client_handles(ref c_flat_handles);
        if (result != 0)
        {
            Debug.Log(string.Format("Controlpads Error (get_client_handles): {0}", result));
        }
        List<string> handles = CFlatToList(c_flat_handles);
        ControlpadsLibrary.free_c_flat_string_vec(c_flat_handles);
        return handles;
    }

    public void SendMessage(string client, string message)
    {
        ControlpadsLibrary.send_message(client, message);
    }

    public List<string> GetMessages(string client)
    {
        c_flat_string_vec c_flat_messages = new c_flat_string_vec();
        int result = ControlpadsLibrary.get_messages(client, ref c_flat_messages);
        if (result != 0)
        {
            Debug.Log(string.Format("Controlpads Error (get_messages): {0}", result));
        }
        List<string> messages = CFlatToList(c_flat_messages);
        ControlpadsLibrary.free_c_flat_string_vec(c_flat_messages);
        return messages;
    }

    // ---- Library Glue Helper ----    
    public List<string> CFlatToList(c_flat_string_vec cFlat)
    {
        List<string> list = new List<string>();
        int char_index = 0;
        for (UInt64 lens_i = 0; lens_i < cFlat.lens_len; lens_i++)
        {
            unsafe
            {
                UInt64* u64ptr = (UInt64*)cFlat.lens_ptr;
                u64ptr += lens_i;
                UInt64 len = *u64ptr;
                string s = Marshal.PtrToStringAnsi(cFlat.chars_ptr + char_index, (int)len);
                char_index += (int)len;
                list.Add(s);
            }
        }
        return list;
    }


}
