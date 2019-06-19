# Azure Functions - Image Watermark

An azure function to add image or text watermark on an image.

## Setup

This code is written for azure function using dot net core. It also contains a visual studio solution and project which can be used to publish/deploy on azure function.

## Request

Once the function is up and running. The request can be passed using http trigger. Following is the format of POST request body.

```JSON
{
    "imageUri" : "{IMAGE_URL}",
    "watermarkUri" : "{WATERMARK_URL}",
    "watermarkText" : "{WATERMARK_TEXT}",
    "watermarkLocation" : {WATERMARK_LOCATION}
}
```

### For Example

```JSON
{
    "imageUri" : "https://hanzla.net/ActualImage.png",
    "watermarkUri" : "https://hanzla.net/WatermarkImage.png",
    "watermarkText" : "This is watermark text",
    "watermarkLocation" : 5
}
```

### Note

If `watermarkUri` and `watermarkText` both are passed then `watermarkUri` will be considered as watermark in final image.

Here the `watermarkLocation` can be any of the following:

| Value  | Position      |
| ------ |:-------------:|
| 0      | Undefined     |
| 0      | Forget        |
| 1      | Northwest     |
| 2      | North         |
| 3      | Northeast     |
| 4      | West          |
| 5      | Center        |
| 6      | East          |
| 7      | Southwest     |
| 8      | South         |
| 9      | Southeast     |

## Response

The response for the POST request contains the image in form of bytes.
