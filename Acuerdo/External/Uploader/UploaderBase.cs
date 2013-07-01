using Dulcet.Twitter;
using Dulcet.Twitter.Credential;

namespace Acuerdo.External.Uploader
{
    /// <summary>
    /// アップロードは行えず、画像URLの解決のみを行えます。
    /// </summary>
    public interface IResolver
    {
        /// <summary>
        /// 指定したURLがこのリゾルバで解決可能かをチェックします。
        /// </summary>
        /// <param name="url">解決するURL</param>
        /// <returns>解決可能ならtrue</returns>
        bool IsResolvable(string url);

        /// <summary>
        /// 指定したURLを解決し、解決結果のURLを返します。<para />
        /// 解決できない場合はNULLを返します。
        /// </summary>
        /// <param name="url">解決対象URL</param>
        /// <returns>解決できた場合、解決結果のURL</returns>
        string Resolve(string url);
    }

    /// <summary>
    /// 画像のアップロードと画像URLの解決を行います。
    /// </summary>
    public interface IUploader : IResolver
    {
        /// <summary>
        /// 画像のアップロードを行います。
        /// </summary>
        /// <param name="accElement">OAuth 接続情報</param>
        /// <param name="path">アップロードする画像パス</param>
        /// <param name="comment">画像に対するコメント</param>
        /// <returns>アップロードが完了した場合、URL</returns>
        string UploadImage(OAuth credential, string path, string comment);

        /// <summary>
        /// このアップロードサービスの名前を取得します。<para />
        /// 唯一になるように名づけなければなりません。
        /// </summary>
        string ServiceName { get; }
    }

    public interface IPostDelegatingUploader : IUploader
    {
        TwitterStatus PostAndUpload(OAuth credential, string path, string body, long? inReplyTo);
    }
}
