module Post.Airtable

let [<Literal>] ListPostsResponse = """
{
  "records": [
    {
      "id": "rec5AMJ3Lah6OSy61",
      "fields": {
        "created_at": "2020-04-04T15:53:36.000Z",
        "content": "## First Post\nHello world.\n",
        "updated_at": "2020-04-04T16:06:47.000Z",
        "images": [
          {
            "id": "attDBmq4n3Dh7nntD",
            "url": "https://dl.airtable.com/.attachments/13bc68d34a8ed265e8554b8845aad48b/a67eaadf/2014-04-1312.38.02.jpg",
            "filename": "benji.jpg",
            "size": 227971,
            "type": "image/jpeg",
            "thumbnails": {
              "small": {
                "url": "https://dl.airtable.com/.attachmentThumbnails/711062d941f16ded67364d96c585d8c7/39d743d6",
                "width": 33,
                "height": 36
              },
              "large": {
                "url": "https://dl.airtable.com/.attachmentThumbnails/fe1cf000747d4ed3158afd3c60d3c5f4/d59fd773",
                "width": 512,
                "height": 565
              },
              "full": {
                "url": "https://dl.airtable.com/.attachmentThumbnails/1018b8fa7aae7e31aceab937dd7c0fb7/30e8080f",
                "width": 3000,
                "height": 3000
              }
            }
          }
        ],
        "title": "First Post"
      },
      "createdTime": "2020-04-04T15:53:36.000Z"
    }
  ],
  "offset": "rec5AMJ3Lah6OSy61"
}
"""

let [<Literal>] GetPostResponse = """
{
  "id": "rec5AMJ3Lah6OSy61",
  "fields": {
    "created_at": "2020-04-04T15:53:36.000Z",
    "content": "## First Post\nHello world.\n",
    "updated_at": "2020-04-04T16:06:47.000Z",
    "images": [
      {
        "id": "attDBmq4n3Dh7nntD",
        "url": "https://dl.airtable.com/.attachments/13bc68d34a8ed265e8554b8845aad48b/a67eaadf/2014-04-1312.38.02.jpg",
        "filename": "benji.jpg",
        "size": 227971,
        "type": "image/jpeg",
        "thumbnails": {
          "small": {
            "url": "https://dl.airtable.com/.attachmentThumbnails/711062d941f16ded67364d96c585d8c7/39d743d6",
            "width": 33,
            "height": 36
          },
          "large": {
            "url": "https://dl.airtable.com/.attachmentThumbnails/fe1cf000747d4ed3158afd3c60d3c5f4/d59fd773",
            "width": 512,
            "height": 565
          },
          "full": {
            "url": "https://dl.airtable.com/.attachmentThumbnails/1018b8fa7aae7e31aceab937dd7c0fb7/30e8080f",
            "width": 3000,
            "height": 3000
          }
        }
      }
    ],
    "title": "First Post"
  },
  "createdTime": "2020-04-04T15:53:36.000Z"
}
"""
