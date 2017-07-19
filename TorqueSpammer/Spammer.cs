using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TorqueSpammer
{
  public class Spammer
  {
    private readonly Uri _rootUri = new Uri("http://localhost:54321/");
    private readonly HttpClient _client = new HttpClient();
    private readonly Random _random = new Random(); //holds up spork
    private const int SpamInterval = 25; //milliseconds

    public Spammer()
    {
      _client.BaseAddress = _rootUri;
      _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
        "Basic",
        Convert.ToBase64String(
          Encoding.ASCII.GetBytes("dev:dev")));
    }

    public void Spam()
    {
      var requestType = (SpamRequestType) _random.Next(0, 7);
      SpamRequest spam;

      switch (requestType)
      {
        case SpamRequestType.EditVehicle:
          spam = GenerateEditVehicle();
          break;
        case SpamRequestType.EditDisplayLot:
          spam = GenerateEditDisplayLot();
          break;
        case SpamRequestType.DeleteDisplayLot:
          spam = GenerateDeleteDisplayLot();
          break;
        case SpamRequestType.EditLot:
          spam = GenerateEditLot();
          break;
        case SpamRequestType.DeleteLot:
          spam = GenerateDeleteLot();
          break;
        case SpamRequestType.AssignSourceFeed:
          spam = GenerateAssignSourceFeed();
          break;
        case SpamRequestType.MapLotSource:
          spam = GenerateMapLotSource();
          break;
        default:
          throw new ThisShouldNeverHappenException("Fuck is you doing here");
      }
      Console.WriteLine($"Gonna spam with a {requestType}");

      var request = new HttpRequestMessage()
      {
        RequestUri = new Uri(_rootUri, spam.Endpoint),
        Method = spam.Method,
        Content = new StringContent(spam.Payload, Encoding.UTF8, "application/json")
      };

      Task.Run(() => MakeRequest(request));

      Thread.Sleep(SpamInterval);
    }

    public async Task MakeRequest(HttpRequestMessage request)
    {
      try
      {
        await _client.SendAsync(request);
      }
      catch (Exception e)
      {
        Console.WriteLine($"Exception making request to {request.RequestUri}\r\nMessage: {e.Message}\r\nStack trace: {e.StackTrace}");
      }
    }

    public class SpamRequest
    {
      public string Endpoint;
      public string Payload;
      public HttpMethod Method;

      public SpamRequest(string endpoint, string payload, HttpMethod method)
      {
        Endpoint = endpoint;
        Payload = payload;
        Method = method;
      }
    }

    public enum SpamRequestType
    {
      EditVehicle,
      EditDisplayLot,
      DeleteDisplayLot,
      EditLot,
      DeleteLot,
      AssignSourceFeed,
      MapLotSource
    }

    public SpamRequest GenerateEditVehicle() => 
      new SpamRequest(
        $"/api/inventory/import/lot-sources/{GenRandInt()}/vehicle/{GenRandString()}",
        "{\"Edits\": [], \"RemoveEdits\": [], \"WhenExpires\": null}",
        HttpMethod.Put
      );

    public SpamRequest GenerateEditDisplayLot() =>
      new SpamRequest("/api/inventory/display-lots/edit",
$@"{{""DisplayLot"":
  {{
    ""DisplayLotId"": {GenRandInt()},
    ""LotOwnerDealerId"": {GenRandInt()},
    ""DealerId"": {GenRandInt()},
    ""LotId"": {GenRandInt()},
    ""LotSourceId"": {GenRandInt()},
    ""LotTypeId"": 1,
    ""LotName"": null,
    ""LotSourceName"": null,
    ""DealerName"": null,
    ""DqlFilter"": null,
    ""DisplayAsMine"": false,
    ""Storage"": null
  }}
}}",
        HttpMethod.Put);

    public SpamRequest GenerateDeleteDisplayLot() => new SpamRequest($"/api/inventory/display-lots/delete/{GenRandInt()}/{GenRandInt()}/{GenRandInt()}", "", HttpMethod.Delete);

    public SpamRequest GenerateEditLot() => new SpamRequest($"/api/inventory/import/lot-sources/{GenRandInt()}/lot/{GenRandInt()}/edit", "{\"Name\": \"\", \"DqlFilter\": \"\", \"DealerId\": 0}", HttpMethod.Put);

    public SpamRequest GenerateDeleteLot() => new SpamRequest($"/api/inventory/import/lot-sources/{GenRandInt()}/lot/{GenRandInt()}/delete", "", HttpMethod.Delete);

    public SpamRequest GenerateAssignSourceFeed() => new SpamRequest($"/api/inventory/import/lot-sources/{GenRandInt()}/source/feeds/{GenRandString(5)}/providers/{GenRandString(5)}", "", HttpMethod.Post);

    public SpamRequest GenerateMapLotSource() => new SpamRequest($"/api/inventory/import/lot-sources/{GenRandInt()}/map", "[]", HttpMethod.Post);


    public int GenRandInt() => _random.Next(100000, 200000); //ddl pk at 68k already oo wee

    public string GenRandString(int length = 17) => Guid.NewGuid().ToString("n").Substring(0, length); //a) who needs letters after F anyway? b) if you call it with a length longer than a single GUID, kablooie
  }

  public class ThisShouldNeverHappenException : Exception
  {
    public ThisShouldNeverHappenException(string message) : base(message) { }
  }
}
