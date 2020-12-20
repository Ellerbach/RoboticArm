using nanoFramework.WebServer;

namespace RoboticArm
{
    /// <summary>
    /// The controller for the arm
    /// </summary>
    [Authentication("Basic:user p@ssw0rd")]
    public class ControllerWebpages
    {
        /// <summary>
        /// Serves the favicon
        /// </summary>
        /// <param name="e">Web server context</param>
        [Route("favicon.ico")]
        public void Favico(WebServerEventArgs e)
        {
            WebServer.SendFileOverHTTP(e.Context.Response, "favico.ico", Resources.GetBytes(Resources.BinaryResources.favico), "image/ico");
        }

        /// <summary>
        /// Servers the script
        /// </summary>
        /// <param name="e">Web server context</param>
        [Route("script.js")]
        public void Script(WebServerEventArgs e)
        {
            e.Context.Response.ContentType = "text/javascript";
            WebServer.OutPutStream(e.Context.Response, Resources.GetString(Resources.StringResources.script));
        }

        /// <summary>
        /// Serves the SVG image
        /// </summary>
        /// <param name="e">Web server context</param>
        [Route("image.svg")]
        public void Image(WebServerEventArgs e)
        {
            WebServer.SendFileOverHTTP(e.Context.Response, "image.svg", Resources.GetBytes(Resources.BinaryResources.image), "image/svg+xml");
        }

        /// <summary>
        /// This is the default page
        /// </summary>
        /// <remarks>the / route *must* always be the last one and the last of the last controller passed 
        /// to the constructor</remarks>
        /// <param name="e">Web server context</param>
        [Route("default.html"), Route("index.html"), Route("/")]
        public void Default(WebServerEventArgs e)
        {
            e.Context.Response.ContentType = "text/html";
            WebServer.OutPutStream(e.Context.Response, Resources.GetString(Resources.StringResources.page));
        }
    }
}
