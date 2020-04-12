module Post.Airtable

let [<Literal>] ListPostsResponse = """
{
    "records": [
        {
            "id": "rec5AMJ3Lah6OSy61",
            "fields": {
                "publish": true,
                "permalink": "win-dev",
                "images": [
                    {
                        "id": "attG20ekJB53TiXxQ",
                        "url": "https://dl.airtable.com/.attachments/7a1c3ea12c11feea435aa52b1dc3567e/26f293fd/cover.png",
                        "filename": "cover.png",
                        "size": 192563,
                        "type": "image/png",
                        "thumbnails": {
                            "small": {
                                "url": "https://dl.airtable.com/.attachmentThumbnails/d402a3279b07d7afe6e86bf46e5e0fec/a209239b",
                                "width": 65,
                                "height": 36
                            },
                            "large": {
                                "url": "https://dl.airtable.com/.attachmentThumbnails/4f81757b8b11eba11a53cf66b818b0dd/b6c8a76b",
                                "width": 640,
                                "height": 353
                            },
                            "full": {
                                "url": "https://dl.airtable.com/.attachmentThumbnails/d91a6369bb2b4a1781f0ea105ab9a8fc/1bfc8523",
                                "width": 3000,
                                "height": 3000
                            }
                        }
                    },
                    {
                        "id": "attjSTTLlWkbdiin8",
                        "url": "https://dl.airtable.com/.attachments/4433dfd413ce8693ad3faf4bb236559e/43abe82d/windows.png",
                        "filename": "windows.png",
                        "size": 29479,
                        "type": "image/png",
                        "thumbnails": {
                            "small": {
                                "url": "https://dl.airtable.com/.attachmentThumbnails/2005a6f0f6f24b9a8f0cc1b044a0d931/f80ba1ac",
                                "width": 52,
                                "height": 36
                            },
                            "large": {
                                "url": "https://dl.airtable.com/.attachmentThumbnails/11668b8cf40f6ba908c11dae9695efac/74a6b8ae",
                                "width": 742,
                                "height": 512
                            },
                            "full": {
                                "url": "https://dl.airtable.com/.attachmentThumbnails/9e9e93edb635664a2b369d780160fc3b/68a8f651",
                                "width": 3000,
                                "height": 3000
                            }
                        }
                    }
                ],
                "title": "First",
                "content": "## First Post\nHello world.\n\n![windows](windows.png)\n",
                "created_at": "2020-04-04T15:53:36.000Z",
                "updated_at": "2020-04-08T11:03:48.000Z"
            },
            "createdTime": "2020-04-04T15:53:36.000Z"
        },
        {
            "id": "recsFf2i7PmQkkrLp",
            "fields": {
                "publish": true,
                "permalink": "second",
                "title": "Second",
                "content": "Second Post\nHello world",
                "created_at": "2020-04-05T18:53:15.000Z",
                "updated_at": "2020-04-08T11:03:57.000Z"
            },
            "createdTime": "2020-04-05T18:53:15.000Z"
        },
        {
            "id": "rec6LKW5GWdOd4oSl",
            "fields": {
                "publish": true,
                "permalink": "third",
                "title": "Third",
                "content": "Third Post\n",
                "created_at": "2020-04-05T23:32:26.000Z",
                "updated_at": "2020-04-08T11:04:04.000Z"
            },
            "createdTime": "2020-04-05T23:32:26.000Z"
        }
    ],
    "offset": "rec6LKW5GWdOd4oSl"
}
"""
