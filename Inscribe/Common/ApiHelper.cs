using System;
using System.Net;
using System.Threading;
using Inscribe.Configuration;
using Inscribe.Storage;

namespace Inscribe.Common
{
    public static class ApiHelper
    {
        /// <summary>
        /// Twitter APIを実行します。<para />
        /// 特定の条件を満たす場合、自動で再実行を試みます。
        /// </summary>
        /// <param name="operate">複数回実行される可能性があるアクションデリゲート</param>
        /// <returns>実行に成功したか</returns>
        public static bool ExecApi(Action operate)
        {
            return ExecApi(() =>
            {
                operate();
                return true;
            });
        }

        /// <summary>
        /// Twitter APIを実行します。<para />
        /// 特定の条件を満たす場合、自動で再実行を試みます。
        /// </summary>
        /// <param name="operate">複数回実行される可能性があるアクションデリゲート</param>
        /// <returns>戻り値(失敗時はデフォルト値が返ります)</returns>
        public static T ExecApi<T>(Func<T> operate)
        {
            for (int i = 0; i < Setting.Instance.ConnectionProperty.AutoRetryCount; i++)
            {
                try
                {
                    return operate();
                }
                catch (WebException we)
                {

                    if (we.Status == WebExceptionStatus.ProtocolError)
                    {
                        var hwr = we.Response as HttpWebResponse;
                        if (hwr != null)
                        {
                            switch (hwr.StatusCode)
                            {
                                case HttpStatusCode.InternalServerError:
                                case HttpStatusCode.ServiceUnavailable:
                                    // Retry
                                    Thread.Sleep(Setting.Instance.ConnectionProperty.AutoRetryIntervalMSec);
                                    continue;
                                case (HttpStatusCode)429:
                                    //Too many requests
                                    return default(T);
                                default:
                                    System.Diagnostics.Debug.WriteLine(we.Message + " / " + hwr.ResponseUri.OriginalString + " / " + hwr.StatusCode);
                                    break;
                            }
                        }
                    }
                    else if (we.Status == WebExceptionStatus.Timeout)
                    {
                        // Retry
                        Thread.Sleep(Setting.Instance.ConnectionProperty.AutoRetryIntervalMSec);
                        continue;
                    }
                    else
                    {
                        ExceptionStorage.Register(we, ExceptionCategory.TwitterError, null, () => operate());
                        return default(T);
                    }
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return default(T);
        }
    }
}
