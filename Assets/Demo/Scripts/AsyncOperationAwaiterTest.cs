using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class AsyncOperationAwaiterTest : MonoBehaviour, IPointerClickHandler
{
    private Texture originalTexture;
    private Texture2D texture;
    private Material material;

    public async void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("downloading...");

        var request = UnityWebRequest.Get("http://placeimg.com/512/512");
        await request.Send();
        Debug.Log("downloaded " + request.downloadedBytes + " bytes");

        texture.LoadImage(request.downloadHandler.data, true);
        material.mainTexture = texture;
    }

    private void Start()
    {
        material = GetComponent<Renderer>().sharedMaterial;
        originalTexture = material.mainTexture;
        texture = new Texture2D(512, 512);

        Debug.Log("\n--> Click on the box to change its texture <--\n");
    }

    private void OnDestroy()
    {
        Destroy(texture);
        material.mainTexture = originalTexture;
    }

    private void Update()
    {
        transform.Rotate(0, 90 * Time.deltaTime, 0);
    }
}