using System;
using System.Collections.Generic;
using Unity.Notifications.Android;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    private Dictionary<ConsumableID, List<int>> notificationIds = new Dictionary<ConsumableID, List<int>>();

    void Start()
    {
        // Crear un canal para las notificaciones si aún no se ha creado
        var channel = new AndroidNotificationChannel()
        {
            Id = "default_channel",
            Name = "Default Channel",
            Importance = Importance.Default,
            Description = "Canal para notificaciones regulares"
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    /// <summary>
    /// Programa una notificación para el dispositivo Android.
    /// </summary>
    /// <param name="title">El titulo de la notificación</param>
    /// <param name="text">El mensaje que mostrará la notificación.</param>
    /// <param name="scheduledTime">La hora a la que se mostrará la notificación.</param>
    /// <param name="category">El identificador de la categoría de la notificación.</param>
    public int ScheduleNotification(string title,string text, DateTime scheduledTime, ConsumableID category)
    {
        var notification = new AndroidNotification()
        {
            Title = title,
            Text = text,
            FireTime = scheduledTime
        };

        // Programar la notificación
        int notificationId = AndroidNotificationCenter.SendNotification(notification, "default_channel");

        // Guardar el ID de la notificación asociada a la categoría
        if (!notificationIds.ContainsKey(category)) {
            notificationIds.Add(category,new List<int>());
        }
        notificationIds[category].Add(notificationId);
        
        return notificationId;
    }

    /// <summary>
    /// Elimina una notificación programada, si existe.
    /// </summary>
    /// <param name="category">La categoría de la notificación que se eliminará.</param>
    public void CancelNotificationsFromCategory(ConsumableID category)
    {
        if (notificationIds.TryGetValue(category, out List<int> notifications))
        {
            foreach (var notificationId in notifications) {
            AndroidNotificationCenter.CancelNotification(notificationId);
            }
            notificationIds.Remove(category);
        }
    }
}
