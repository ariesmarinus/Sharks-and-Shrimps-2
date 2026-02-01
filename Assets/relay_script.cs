using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class relay_script : MonoBehaviour
{
    public TMP_InputField the_code;
    public Button join_button;
    public GameObject relay_screen;
    public TMP_Text code_created;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        join_button.onClick.AddListener(() => JoinRelay(the_code.text));
    }

    public async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            string join_code = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(join_code);
            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
            code_created.text = join_code.ToString();
            relay_screen.SetActive(false);
        }
        catch(RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinRelay(string the_code)
    {
        try
        {
            Debug.Log("join relay with" + the_code);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(the_code);
            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
            relay_screen.SetActive(false);
        }
        catch(RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
