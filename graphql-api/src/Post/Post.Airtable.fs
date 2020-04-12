module Post.Airtable

let [<Literal>] ListPostsResponse = """
{
    "records": [
        {
            "id": "reclAnGWapIS5ZG5K",
            "fields": {
                "status": "Published",
                "permalink": "about",
                "title": "About",
                "content": "Welcome to the personal blog of Andrew C. Meier! Originally from STL, now living in NYC",
                "created_at": "2020-04-12T16:55:28.000Z",
                "updated_at": "2020-04-12T18:07:32.000Z"
            },
            "createdTime": "2020-04-12T16:55:28.000Z"
        },
        {
            "id": "rec5AMJ3Lah6OSy61",
            "fields": {
                "status": "Published",
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
                        "filename": "computer.png",
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
                    },
                    {
                        "id": "att3NIcUtCNCMoYgC",
                        "url": "https://dl.airtable.com/.attachments/2d6c88104f2b2f8802277f6dbb098a26/94aed48e/screen-to-gif.gif",
                        "filename": "screen-to-gif.gif",
                        "size": 35362,
                        "type": "image/gif",
                        "thumbnails": {
                            "small": {
                                "url": "https://dl.airtable.com/.attachmentThumbnails/8cb9d6899882d1fb0cccbf811282fcc6/cd6c8b53",
                                "width": 96,
                                "height": 36
                            },
                            "large": {
                                "url": "https://dl.airtable.com/.attachmentThumbnails/1b8eec2b2135a3a1d809cd096c5dbe75/1e8b8528",
                                "width": 724,
                                "height": 271
                            },
                            "full": {
                                "url": "https://dl.airtable.com/.attachmentThumbnails/aea0ee2598a5ba239dbdc4c7ce8a7ba4/089bf51e",
                                "width": 3000,
                                "height": 3000
                            }
                        }
                    },
                    {
                        "id": "atthcnthQJPZjvrbv",
                        "url": "https://dl.airtable.com/.attachments/8bef4e7c40ce1ad153193cf6e603658e/cbff01f0/kubernetes.png",
                        "filename": "kubernetes.png",
                        "size": 62520,
                        "type": "image/png",
                        "thumbnails": {
                            "small": {
                                "url": "https://dl.airtable.com/.attachmentThumbnails/3e14844f8443ad6a28b63f680ed89438/b3aad183",
                                "width": 47,
                                "height": 36
                            },
                            "large": {
                                "url": "https://dl.airtable.com/.attachmentThumbnails/d19a7c95ab5da71103ee2b074a5704a8/f1ab55ba",
                                "width": 664,
                                "height": 512
                            },
                            "full": {
                                "url": "https://dl.airtable.com/.attachmentThumbnails/ba8405d387ea92e90723db7cf5f2b450/47d4654b",
                                "width": 3000,
                                "height": 3000
                            }
                        }
                    }
                ],
                "title": "Windows Development Environment",
                "content": "## Table of Contents\n- [Computer and Windows](#computer-and-windows): Recommended specs for computer",
                "created_at": "2020-04-04T15:53:36.000Z",
                "updated_at": "2020-04-12T18:08:23.000Z"
            },
            "createdTime": "2020-04-04T15:53:36.000Z"
        }
    ],
    "offset": "rec5AMJ3Lah6OSy61"
}
"""
