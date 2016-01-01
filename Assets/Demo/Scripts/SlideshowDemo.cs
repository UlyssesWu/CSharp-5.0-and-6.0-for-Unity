using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SlideshowDemo : MonoBehaviour
{
	public RawImage rawImage;
	public Text counterText;
	public Button startButton;
	public Button abortButton;
	[Range(.1f, 5)]
	public float delayInSeconds = 1;

	private Texture2D texture;
	private CancellationTokenSource tokenSource;

	private void Start()
	{
		texture = new Texture2D(320, 200);
		rawImage.texture = texture;
	}

	private void Destroy()
	{
		Destroy(texture);
	}

	public async void StartSlideshow() // async methods can be on-click handlers, coroutines can't
	{
		tokenSource = new CancellationTokenSource();

		startButton.interactable = false;
		abortButton.interactable = true;

		try
		{
			await ShowRandomImages(Random.Range(2, 21), tokenSource.Token);
		}
		catch (TaskCanceledException)
		{
			Debug.Log("Aborted");
		}
		finally
		{
			startButton.interactable = true;
			abortButton.interactable = false;
		}
	}

	public void AbortSlideshow()
	{
		abortButton.interactable = false;
		tokenSource.Cancel();
	}

	private async Task ShowRandomImages(int count, CancellationToken cancellationToken)
	{

		for (int i = 0; i < count; i++)
		{
			var image = await AsyncTools.DownloadAsBytesAsync("http://placeimg.com/320/200", cancellationToken);
			texture.LoadImage(image);
			rawImage.SetNativeSize();

			counterText.text = $"{i + 1} of {count}";

			if (i != count - 1)
			{
				await TaskEx.Delay(TimeSpan.FromSeconds(delayInSeconds), cancellationToken);
			}
		}
	}
}