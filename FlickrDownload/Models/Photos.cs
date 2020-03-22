using System.Collections.Generic;

public class PhotoApiResponse
{
    public PhotoCollection Photos { get; set; }
}

public class PhotoCollection
{
    public List<Photo> Photo { get; set; }
}

public class Photo
{
    public string Id { get; set; }

    public string Url_s { get; set; }

    public string Url_m { get; set; }

    public string Url_l { get; set; }

    public string Title { get; set; }

    public string ImageHiddenClass => ImageHidden ? "hidden" : "";

    public bool ImageHidden { get; set; } = true;
}