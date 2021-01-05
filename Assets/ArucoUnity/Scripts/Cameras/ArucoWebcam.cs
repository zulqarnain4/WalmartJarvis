﻿using System;
using UnityEngine;

namespace ArucoUnity.Cameras
{
  /// <summary>
  /// Captures images of a webcam.
  /// </summary>
  public class ArucoWebcam : ArucoCamera
  {
    // Constants

    protected const int cameraId = 0;

    // Editor fields

    [SerializeField]
    [Tooltip("The id of the webcam to use.")]
    private int webcamId;

    // IArucoCamera properties

    public override int CameraNumber { get { return 1; } }

    public override string Name { get; protected set; }

    // Properties

    /// <summary>
    /// Gets or set the id of the webcam to use.
    /// </summary>
    public int WebcamId { get { return webcamId; } set { webcamId = value; } }

    /// <summary>
    /// Gets the controller of the webcam to use.
    /// </summary>
    public WebcamController WebcamController { get; private set; }

    // MonoBehaviour methods

    /// <summary>
    /// Initializes <see cref="WebcamController"/> and subscribes to.
    /// </summary>
    protected override void Awake()
    {
      base.Awake();
      WebcamController = gameObject.AddComponent<WebcamController>();
      WebcamController.Started += WebcamController_Started;
    }

    /// <summary>
    /// Unsubscribes to <see cref="WebcamController"/>.
    /// </summary>
    protected override void OnDestroy()
    {
      base.OnDestroy();
      WebcamController.Started -= WebcamController_Started;
    }

    // ConfigurableController methods

    /// <summary>
    /// Calls <see cref="WebcamController.Configure"/> and sets <see cref="Name"/>.
    /// </summary>
    protected override void Configuring()
    {
      base.Configuring();

      WebcamController.Ids.Clear();
      WebcamController.Ids.Add(WebcamId);
      WebcamController.Configure();

      Name = WebcamController.Devices[cameraId].name;
    }

    /// <summary>
    /// Calls <see cref="WebcamController.StartWebcams"/>.
    /// </summary>
    protected override void Starting()
    {
      base.Starting();
      WebcamController.StartWebcams();
    }

    /// <summary>
    /// Calls <see cref="WebcamController.StopWebcams"/>.
    /// </summary>
    protected override void Stopping()
    {
      base.Stopping();
      WebcamController.StopWebcams();
    }

    /// <summary>
    /// Blocks <see cref="ArucoCamera.OnStarted"/> until <see cref="WebcamController.IsStarted"/>.
    /// </summary>
    protected override void OnStarted()
    {
    }

        // ArucoCamera methods

        /// <summary>
        /// Copy current webcam images to <see cref="ArucoCamera.NextImages"/>.
        /// </summary>
        public Material undistort;
        Texture2D temp;
        protected override bool UpdatingImages()
    {

            //HACK: cut byte array in half
            int sourceWidth = WebcamController.Textures2D[cameraId].width;
            int sourceHeight = WebcamController.Textures2D[cameraId].height;

            byte[] croppedImage = CropImageArray(WebcamController.Textures2D[cameraId].GetRawTextureData(), sourceWidth, 24, sourceWidth / 2, sourceHeight);

            //HACK: Load to texture to test undistort shader
            if (temp == null) {
                temp = new Texture2D(1280, 960, TextureFormat.RGB24, false);
            }
            temp.LoadRawTextureData(croppedImage);
            temp.Apply();
            undistort.mainTexture = temp;

            Array.Copy(croppedImage, NextImageDatas[cameraId], ImageDataSizes[cameraId]);

            //Array.Copy(WebcamController.Textures2D[cameraId].GetRawTextureData(), NextImageDatas[cameraId], ImageDataSizes[cameraId]);
            return true;
    }

    byte[] CropImageArray(byte[] pixels, int sourceWidth, int bitsPerPixel, int newWidth, int newHeight) {
        var blockSize = bitsPerPixel / 8;
        var outputPixels = new byte[newWidth * newHeight * blockSize];

        //Create the array of bytes.
        for (var line = 0; line <= newHeight - 1; line++) {
            var sourceIndex = line * sourceWidth * blockSize;
            var destinationIndex = line * newWidth * blockSize;

            Array.Copy(pixels, sourceIndex, outputPixels, destinationIndex, newWidth * blockSize);
        }

        return outputPixels;
    }

        // Methods

        /// <summary>
        /// Configures <see cref="ArucoCamera.Textures"/> and calls <see cref="ArucoCamera.OnStarted"/>.
        /// </summary>
        protected virtual void WebcamController_Started(WebcamController webcamController)
    {
      var webcamTexture = WebcamController.Textures2D[cameraId];
            //HACK: Textures[cameraId] = new Texture2D(webcamTexture.width, webcamTexture.height, webcamTexture.format, false);
            print("FORMAT: " + webcamTexture.format);
            Textures[cameraId] = new Texture2D(webcamTexture.width/2, webcamTexture.height, webcamTexture.format, false);
            base.OnStarted();
    }
  }
}