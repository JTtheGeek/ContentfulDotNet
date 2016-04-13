## Synopsis

ContentfulDotNetApi is a simple to use, lightweight api built in .net framework 4.5 for accessing contentful's cms as a service.

## Code Example

Create a simple class with properties that match your content type fields. Also ensure the class has a string property for the id.  Even if contentful field name starts with a lowercase character, you can use an uppercase character for the property name and the API will auto map it.

Also be aware if your field is a link to another content item, the value of the field will instead by the id of the referenced object.

'''public class ExampleContentType
{
    public String Id { get; set; }
    public String Title { get; set; } 
    public int Description { get; set; }
}

var contentGateway = await ContentfulGatewayFactory.GetGateway(contentfulApiKey, contentfulSpaceId);
var exampleContentTypeItems = await contentGateway.GetObjectsByContentType<ExampleContentType>("contentTypeId");'''

## Notes

This is a first commit, it does work flawlessly in my projects, but I'm not pushing it too hard.  Here's a list of a things I think will need to be added
    * support for queries where returned items > 100, currently assumes the request result contains all the items matching
    * support for paginated queries
    * rate limiting
    * built in cache layer?



## License

Free For All, no attribution required.
