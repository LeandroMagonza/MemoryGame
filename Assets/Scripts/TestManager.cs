using System.Collections;
#if UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android;
#endif
using UnityEngine;

public class TestManager : MonoBehaviour
{
    private NotificationManager notificationManager;

    void Awake()
    {
        notificationManager = FindObjectOfType<NotificationManager>();
        if (notificationManager == null)
        {
            Debug.LogError("NotificationManager not found in the scene.");
        }
    }

    [ContextMenu("Run Notification Test")]
    public void RunNotificationTest()
    {
        if (notificationManager == null)
        {
            Debug.LogError("NotificationManager is not assigned.");
            return;
        }

        StartCoroutine(NotificationTestCoroutine());
    }

    private IEnumerator NotificationTestCoroutine()
    {
        string title = "Test Notification";
        string text = "This is a test notification.";
        System.DateTime scheduledTime = System.DateTime.Now.AddSeconds(5);
        ConsumableID category = ConsumableID.Clue;

        // Schedule the notification
        int notificationId = notificationManager.ScheduleNotification(title, text, scheduledTime, category);

        // Check if the notification was created
        yield return new WaitForSeconds(1); // Wait for a short moment to ensure the notification is registered

        if (!notificationManager.HasNotification(notificationId, category))
        {
            Debug.LogError("Notification creation failed.");
            yield break;
        }
        Debug.Log("Notification created successfully.");

        // Cancel the notification
        notificationManager.CancelNotificationsFromCategory(category);

        // Check if the notification was removed
        yield return new WaitForSeconds(1); // Wait for a short moment to ensure the notification is cancelled

        if (notificationManager.HasNotification(notificationId, category))
        {
            Debug.LogError("Notification cancellation failed.");
        }
        else
        {
            Debug.Log("Notification cancelled successfully.");
        }
    }
}
