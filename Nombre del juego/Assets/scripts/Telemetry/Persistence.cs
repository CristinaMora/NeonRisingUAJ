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
    protected PersistenceType persType;   // Tipo de persistencia de los eventos


    public Persistence(PersistenceType _persType, Format _format)
    {
        persType = _persType;
        format = _format;
    }

    // --- SEND EVENT ---
    public virtual async void SendEvent(Event e) { }

    // --- FLUSH QUEUE ---
    public virtual void FlushQueue(ConcurrentQueue<Event> queue) { }

}
