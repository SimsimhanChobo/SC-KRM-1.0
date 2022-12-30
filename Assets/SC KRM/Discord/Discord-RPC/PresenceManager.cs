using System;
using UnityEngine;
using UnityEngine.Events;

namespace DiscordPresence
{
    [Serializable]
    public class DiscordJoinEvent : UnityEvent<string> { }

    [Serializable]
    public class DiscordSpectateEvent : UnityEvent<string> { }

    [Serializable]
    public class DiscordJoinRequestEvent : UnityEvent<DiscordRpc.JoinRequest> { }

    public class PresenceManager : MonoBehaviour
    {
        public DiscordRpc.RichPresence presence = new DiscordRpc.RichPresence();
        public string applicationId;
        public string optionalSteamId;
        public int callbackCalls;
        //public int clickCounter;
        public DiscordRpc.JoinRequest joinRequest;
        public UnityEvent onConnect;
        public UnityEvent onDisconnect;
        public UnityEvent hasResponded;
        public DiscordJoinEvent onJoin;
        public DiscordJoinEvent onSpectate;
        public DiscordJoinRequestEvent onJoinRequest;

        DiscordRpc.EventHandlers handlers;

        public static PresenceManager instance;

        /*public void OnClick()
        {
            Debug.Log("Discord: on click!");
            clickCounter++;

            presence.details = string.Format("Button clicked {0} times", clickCounter);

            DiscordRpc.UpdatePresence(presence);
        }*/

        public void RequestRespondYes()
        {
            Debug.Log("responding yes to Ask to Join request", "Discord");
            DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.Yes);
            hasResponded.Invoke();
        }

        public void RequestRespondNo()
        {
            Debug.Log("responding no to Ask to Join request", "Discord");
            DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.No);
            hasResponded.Invoke();
        }

        #region Discord Callbacks
        public void ReadyCallback()
        {
            ++callbackCalls;
            Debug.ForceLog("ready", "Discord");
            onConnect.Invoke();
            UpdatePresence(null);
        }

        public void DisconnectedCallback(int errorCode, string message)
        {
            ++callbackCalls;
            Debug.Log(string.Format("disconnect {0}: {1}", errorCode, message), "Discord");
            onDisconnect.Invoke();
        }

        public void ErrorCallback(int errorCode, string message)
        {
            ++callbackCalls;
            Debug.Log(string.Format("error {0}: {1}", errorCode, message), "Discord");
        }

        public void JoinCallback(string secret)
        {
            ++callbackCalls;
            Debug.Log(string.Format("join ({0})", secret), "Discord");
            onJoin.Invoke(secret);
        }

        public void SpectateCallback(string secret)
        {
            ++callbackCalls;
            Debug.Log(string.Format("spectate ({0})", secret), "Discord");
            onSpectate.Invoke(secret);
        }

        public void RequestCallback(ref DiscordRpc.JoinRequest request)
        {
            ++callbackCalls;
            Debug.Log(string.Format("join request {0}#{1}: {2}", request.username, request.discriminator, request.userId), "Discord");
            joinRequest = request;
            onJoinRequest.Invoke(request);
        }
        #endregion

        #region Monobehaviour Callbacks
        // Singleton
        public virtual void Awake()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);

            //DontDestroyOnLoad(gameObject);
#else
            enabled = false;
            DestroyImmediate(gameObject);
#endif
        }

        public virtual void Update()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            DiscordRpc.RunCallbacks();
#endif
        }

        public virtual void OnEnable()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            Debug.ForceLog("init", "Discord");
            callbackCalls = 0;

            handlers = new DiscordRpc.EventHandlers();
            handlers.readyCallback = ReadyCallback;
            handlers.disconnectedCallback += DisconnectedCallback;
            handlers.errorCallback += ErrorCallback;
            handlers.joinCallback += JoinCallback;
            handlers.spectateCallback += SpectateCallback;
            handlers.requestCallback += RequestCallback;
            DiscordRpc.Initialize(applicationId, ref handlers, true, optionalSteamId);
#endif
        }

        public virtual void OnDisable()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            Debug.ForceLog("shutdown", "Discord");
            DiscordRpc.Shutdown();
#endif
        }
        #endregion

        #region Update Presence Method
        public static void UpdatePresence(string detail, string state = null, long start = -1, long end = -1, string largeKey = null, string largeText = null,
            string smallKey = null, string smallText = null, string partyId = null, int size = -1, int max = -1, string match = null, string join = null,
            string spectate = null/*, bool instance*/)
        {
            instance.presence.details = detail ?? instance.presence.details;
            instance.presence.state = state ?? instance.presence.state;
            instance.presence.startTimestamp = (start == -1) ? instance.presence.startTimestamp : start;
            instance.presence.endTimestamp = (end == -1) ? instance.presence.endTimestamp : end;
            instance.presence.largeImageKey = largeKey ?? instance.presence.largeImageKey;
            instance.presence.largeImageText = largeText ?? instance.presence.largeImageText;
            instance.presence.smallImageKey = smallKey ?? instance.presence.smallImageKey;
            instance.presence.smallImageText = smallText ?? instance.presence.smallImageText;
            instance.presence.partyId = partyId ?? instance.presence.partyId;
            instance.presence.partySize = (size == -1) ? instance.presence.partySize : size;
            instance.presence.partyMax = (max == -1) ? instance.presence.partyMax : max;
            instance.presence.matchSecret = match ?? instance.presence.matchSecret;
            instance.presence.joinSecret = join ?? instance.presence.joinSecret;
            instance.presence.spectateSecret = spectate ?? instance.presence.spectateSecret;
            //instance.presence.presence.instance =
            DiscordRpc.UpdatePresence(instance.presence);
        }

        public static void ClearPresence()
        {
            instance.presence.details = "";
            instance.presence.state = "";
            instance.presence.startTimestamp = 0;
            instance.presence.endTimestamp = 0;
            instance.presence.largeImageKey = "";
            instance.presence.largeImageText = "";
            instance.presence.smallImageText = "";
            instance.presence.smallImageKey = "";
            instance.presence.partyId = "";
            instance.presence.partySize = 0;
            instance.presence.partyMax = 0;
            instance.presence.matchSecret = "";
            instance.presence.joinSecret = "";
            instance.presence.spectateSecret = "";
            //instance.presence.instance =
        }

        public static void ClearAndUpdate()
        {
            ClearPresence();
            DiscordRpc.UpdatePresence(instance.presence);
        }
        #endregion
    }
}
