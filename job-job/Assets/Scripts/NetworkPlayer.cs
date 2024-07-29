using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class NetworkPlayer : NetworkBehaviour
{
    // references
    [SerializeField] private Avatar[] avatars;
    private NavMeshAgent navMeshAgent;
    [SerializeField] private TMP_Text playerNameText;
    private PlayerManager playerManager;

    // network vars

    public NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>("Name", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> avatarIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Vector3> targetPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    // job job
    public NetworkVariable<FixedString512Bytes> question = new NetworkVariable<FixedString512Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<FixedString512Bytes> answer = new NetworkVariable<FixedString512Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<FixedString512Bytes> fragments = new NetworkVariable<FixedString512Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);



    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        // do some setup if this is the local player

        Debug.Log("NetworkPlayer Start");
        Debug.Log("IsLocalPlayer: " + IsLocalPlayer);

        if (!IsLocalPlayer)
            return;

        // link to the player manager
        playerManager = FindObjectOfType<PlayerManager>();
        playerManager.LinkNetworkPlayer(this);

        // testing nav mesh
        StartCoroutine(RandomWalkCoroutine());
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn: " + playerName.Value);

        // Set avatar based on avatarIndex.Value
        SetAvatar(avatarIndex.Value);

        avatarIndex.OnValueChanged += SetAvatar;

        SetNameTMP(playerName.Value);
        playerName.OnValueChanged += SetNameTMP;

        targetPosition.OnValueChanged += (previous, current) =>
        {
            navMeshAgent.SetDestination(current);
        };

        // job job
        question.OnValueChanged += OnQuestionChanged;
        answer.OnValueChanged += OnAnswerChanged;
        fragments.OnValueChanged += OnFragmentsChanged;

    }

    #region Job Job

    private void OnQuestionChanged(FixedString512Bytes previous, FixedString512Bytes current)
    {
        Debug.Log("Question changed from " + previous + " to " + current);
        if (IsLocalPlayer)
        {
            playerManager.JJ_SetQuestion(current.ToString());
        }
    }

    private void OnAnswerChanged(FixedString512Bytes previous, FixedString512Bytes current)
    {
        Debug.Log("Answer changed from " + previous + " to " + current);
    }

    private void OnFragmentsChanged(FixedString512Bytes previous, FixedString512Bytes current)
    {
        Debug.Log("Fragments changed from " + previous + " to " + current);
        if (IsLocalPlayer)
        {
            playerManager.JJ_SetFragments(current.ToString());
        }
    }


    #endregion


    #region Avatars and Customization
    public void SetAvatar(int index)
    {
        Debug.Log("Setting avatar for " + playerName.Value + " to " + index);

        // for some reason, stickman is not being set to inactive (index 0)

        // lets set it to inactive here to see if that fixes it
        avatars[0].gameObject.SetActive(false);
        // that fixed it. just gonna leave it like this for now

        // Set the avatar based on the index
        avatars[index].gameObject.SetActive(true);
    }

    public void SetAvatar(int prev, int current)
    {
        // Set the previous avatar to inactive
        avatars[prev].gameObject.SetActive(false);

        SetAvatar(current);
    }

    public void SetNameTMP(FixedString64Bytes name)
    {
        playerNameText.text = name.ToString();
    }
    public void SetNameTMP(FixedString64Bytes prev, FixedString64Bytes current)
    {
        SetNameTMP(current);
    }

    #endregion

    private IEnumerator RandomWalkCoroutine()
    {
        while (true)
        {
            targetPosition.Value = transform.position + new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));

            yield return new WaitForSeconds(5);
        }
    }
}
