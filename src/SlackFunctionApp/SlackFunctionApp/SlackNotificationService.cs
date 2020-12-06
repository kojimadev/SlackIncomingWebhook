using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SlackFunctionApp
{
	/// <summary>
	/// Slackに通知するサービス
	/// </summary>
	public class SlackNotificationService
	{
		#region 内部クラス

		/// <summary>
		/// Slackに送信するjsonの変換元となるデータを保持するクラス
		/// 送信チャンネルはデフォルト
		/// </summary>
		private class SlackPayload
		{
			/// <summary>
			/// ユーザ名
			/// </summary>
			public string username { get; set; } = string.Empty;

			/// <summary>
			/// 本文
			/// </summary>
			public string text { get; set; } = string.Empty;

			/// <summary>
			/// 対象チャンネル名
			/// </summary>
			public string channel { get; set; } = string.Empty;
		}

		#endregion

		#region 定数

		/// <summary>
		/// ペイロード用のKey値
		/// JsonをUrlに変換する際に必要なデータをディクショナリに詰める必要がある。
		/// その際にペイロードの値はこのKey値で格納する。
		/// </summary>
		private const string PayloadKeyValue = "payload";

		/// <summary>
		/// Channelメンションを付けるための文字列
		/// </summary>
		private const string ChannelMention = "<!channel>";

		#endregion

		#region フィールド

		/// <summary>
		/// SlackにPOSTするために使用するHttpClient
		///
		/// 接続先のホストは[チーム名].slack.comとなるため、HttpClientは単一インスタンスを利用する
		/// </summary>
		private static readonly HttpClient _HttpClient = new HttpClient();

		#endregion

		#region 公開サービス

		/// <summary>
		/// メッセージを通知する
		/// </summary>
		/// <param name="message">メッセージ</param>
		/// <param name="webhookUrl">webhookUrl</param>
		/// <param name="userName">ユーザー名</param>
		/// <param name="channelName">チャンネル名(チャンネル名が指定可能なツールのみ対応)</param>
		/// <param name="mention">メンションを付けるか(メンションが可能なツールのみ対応)</param>
		/// <returns>通知に成功したか</returns>
		public async Task<bool> Notify(string message, string webhookUrl, string userName,
			string channelName, bool mention = true)
		{
			// 先頭にChannelメンションを付ける
			if (mention)
			{
				message = $"{ChannelMention}{Environment.NewLine}{message}";
			}

			// 送信パラメータを作成
			var slackPayload = new SlackPayload()
			{
				text = message,
				username = userName,
				channel = channelName,
			};

			// 送信パラメータオブジェクトをJSONに変換
			var jsonSlackPayload = JsonConvert.SerializeObject(slackPayload);

			// Jsonの送信パラメータ情報をURLに変換
			var slackPayloadDictionary = new Dictionary<string, string>()
			{
				{PayloadKeyValue, jsonSlackPayload}
			};
			var urlParameter = new FormUrlEncodedContent(slackPayloadDictionary);

			try
			{
				var response = await _HttpClient.PostAsync(webhookUrl, urlParameter).ConfigureAwait(false);
				return (response.StatusCode == HttpStatusCode.OK);
			}
			catch
			{
				return false;
			}
		}

		#endregion
	}
}