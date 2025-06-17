using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using UnityEngine;
using static Tracker;

public class Persistence
{
    protected Format format;              // Formato de escritura de los eventos


    public Persistence(Format _format)
    {
        format = _format;
    }

    // --- SEND EVENT ---
    public virtual async void SendEvent(Event e) { }

    // --- FLUSH QUEUE ---
    public virtual void FlushQueue(ConcurrentQueue<Event> queue) { }

}
