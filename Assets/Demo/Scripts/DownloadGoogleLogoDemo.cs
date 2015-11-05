using UnityEngine;
using UnityEngine.UI;

public class DownloadGoogleLogoDemo : MonoBehaviour
{
	public RawImage rawImage;
	public Button button;

	public async void ShowGoogleLogosAsync() // can be on-click handler
	{
		button.interactable = false;

		var texture = new Texture2D(1, 1);
		rawImage.texture = texture;
		for (int i = 1; i <= 11; i++)
		{
			await .5f;
			var png = await AsyncTools.DownloadAsBytesAsync($"http://www.google.com/images/srpr/logo{i}w.png");

			texture.LoadImage(png);
			rawImage.SetNativeSize();
		}

		button.interactable = true;
	}

	private void Update()
	{
		transform.Rotate(0, 90 * Time.deltaTime, 0);
	}
}