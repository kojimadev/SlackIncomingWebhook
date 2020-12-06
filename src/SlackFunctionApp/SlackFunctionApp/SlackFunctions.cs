using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SlackFunctionApp
{
	/// <summary>
	/// SlackのAPIを実行する Azure Functions を実現するクラス
	/// </summary>
	public static class SlackFunctions
	{
		/// <summary>
		/// Slack の Incoming Webhook を用いて Slack の指定したチャンネルにメッセージを投稿する
		/// </summary>
		/// <param name="req"></param>
		/// <param name="log"></param>
		/// <returns></returns>
		[FunctionName("slackMessages")]
		public static async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
			ILogger log)
		{
			log.LogInformation("C# HTTP trigger function processed a request.");

			// リクエストボディからパラメータを取得
			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			dynamic data = JsonConvert.DeserializeObject(requestBody);
			string message = data?.message;
			string channelName = data?.channelName;
			bool mention = data?.mention;

			// Slackの指定のチャンネルに投稿する
			// (webhookUrlの値は Incoming Webhook を用いて取得する)
			var service = new SlackNotificationService();
			var success = await service.Notify(message,
				"https://hooks.slack.com/services/XXXXXXX", 
				"BotName", channelName, mention);

			// Slackへの投稿が成功か失敗か返す
			string responseMessage = success ? "success" : "failed";
			return new OkObjectResult(responseMessage);
		}
	}
}