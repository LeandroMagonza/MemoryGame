using System;
using System.Collections.Generic;
using Unity.Notifications.Android;
using UnityEngine;

using System;
using System.Collections.Generic;
using Unity.Notifications.Android;
using UnityEngine;
using UnityEngine.Android;

public class NotificationManager : MonoBehaviour
{
    #region Singleton
    private static NotificationManager _instance;
    public static NotificationManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<NotificationManager>();
            if (_instance == null)
                CustomDebugger.LogError("Singleton<" + typeof(NotificationManager) + "> instance has been not found.");
            return _instance;
        }
    }
    protected void Awake()
    {
        if (_instance == null)
        {
            _instance = this as NotificationManager;
        }
        else if (_instance != this)
            DestroySelf();
    }
    private void DestroySelf()
    {
        if (Application.isPlaying)
            Destroy(this);
        else
            DestroyImmediate(this);
    }
    #endregion

    private Dictionary<ConsumableID, Dictionary<int, ScheduledNotification>> notificationData = new Dictionary<ConsumableID, Dictionary<int, ScheduledNotification>>();

    void Start() {
        AndroidNotificationCenter.Initialize();
        AndroidNotificationCenter.CancelAllDisplayedNotifications();
        // Crear un canal para las notificaciones si aún no se ha creado
        var channel = new AndroidNotificationChannel()
        {
            Id = "default_channel",
            Name = "Default Channel",
            Importance = Importance.Default,
            Description = "Canal para notificaciones regulares"
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
        //ScheduleNotification("TEST", "TEST", DateTime.Now.AddSeconds(5), ConsumableID.Bomb);

    }

    /// <summary>
    /// Programa una notificación para el dispositivo Android.
    /// </summary>
    /// <param name="title">El titulo de la notificación</param>
    /// <param name="text">El mensaje que mostrará la notificación.</param>
    /// <param name="scheduledTime">La hora a la que se mostrará la notificación.</param>
    /// <param name="category">El identificador de la categoría de la notificación.</param>
    public int ScheduleNotification(string title, string text, DateTime scheduledTime, ConsumableID category)
    {
        var notification = new AndroidNotification()
        {
            Title = title,
            Text = text,
            FireTime = scheduledTime
        };

        // Programar la notificación
#if UNITY_ANDROID
        ;
        int notificationId = AndroidNotificationCenter.SendNotification(notification, "default_channel");
        // Crear una nueva notificación programada
        var scheduledNotification = new ScheduledNotification(title, text, scheduledTime);

        // Guardar la notificación programada asociada a la categoría y el ID de la notificación
        if (!notificationData.ContainsKey(category))
        {
            notificationData[category] = new Dictionary<int, ScheduledNotification>();
        }
        notificationData[category][notificationId] = scheduledNotification;
        
        Debug.Log("Schedule Notification: id:"+notificationId+" cat:"+category+"\n"+title+"\n"+text+"\n"+scheduledTime);
        return notificationId;
#endif
#if UNITY_EDITOR
        CustomDebugger.Log("Schedule Notification: "+category+"\n"+title+"\n"+text+"\n"+scheduledTime);
#endif            
        return 0;
    }

    /// <summary>
    /// Elimina una notificación programada, si existe.
    /// </summary>
    /// <param name="category">La categoría de la notificación que se eliminará.</param>
    public void CancelNotificationsFromCategory(ConsumableID category)
    {
        if (notificationData.TryGetValue(category, out Dictionary<int, ScheduledNotification> notifications))
        {
            foreach (var notificationId in notifications.Keys)
            {
                if (notificationId == 0) {
                    continue;
                }
                AndroidNotificationCenter.CancelNotification(notificationId);
            }
            notificationData.Remove(category);
        }
    }
    public bool HasNotification(int notificationId, ConsumableID category)
    {
        if (notificationData.TryGetValue(category, out Dictionary<int, ScheduledNotification> notifications))
        {
            return notifications.ContainsKey(notificationId);
        }
        return false;
    }
    public void AskForPermission()//call this function to ask request
    {
        if (Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            print("permission granted!!");
        }
        else
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
            callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
            callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS", callbacks);
        }

    }
    internal void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionDeniedAndDontAskAgain");
    }

    internal void PermissionCallbacks_PermissionGranted(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionGranted");
    }

    internal void PermissionCallbacks_PermissionDenied(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionDenied");
    }

}

public class ScheduledNotification
{
    public string Title { get; set; }
    public string Text { get; set; }
    public DateTime ScheduledTime { get; set; }

    public ScheduledNotification(string title, string text, DateTime scheduledTime)
    {
        Title = title;
        Text = text;
        ScheduledTime = scheduledTime;
    }
}
