using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Refit;
using Newtonsoft.Json;

public interface IPostmanEchoApi
{
    [Post("/post")]
    Task<PostmanEchoPostResponse> Post([Body] TagRequest request);
}

public class TagRequest
{
    public IEnumerable<string> Objects { get; set; }

    public TagRequest(IEnumerable<string> tags) => Objects = tags;
}

public class PostmanEchoPostResponse
{
    public TagRequest Json { get; set; }
}