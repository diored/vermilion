using System.Runtime.CompilerServices;

// Backward-compatible type forwarding after extracting contracts into DioRed.Vermilion.Abstractions.
[assembly: TypeForwardedTo(typeof(DioRed.Vermilion.ChatId))]
[assembly: TypeForwardedTo(typeof(DioRed.Vermilion.ChatMetadata))]
[assembly: TypeForwardedTo(typeof(DioRed.Vermilion.UserRole))]
[assembly: TypeForwardedTo(typeof(DioRed.Vermilion.ChatStorage.IChatStorage))]
[assembly: TypeForwardedTo(typeof(DioRed.Vermilion.Connectors.IConnector))]
[assembly: TypeForwardedTo(typeof(DioRed.Vermilion.Connectors.MessagePostedEventArgs))]
[assembly: TypeForwardedTo(typeof(DioRed.Vermilion.Connectors.PostResult))]
[assembly: TypeForwardedTo(typeof(DioRed.Vermilion.Interaction.Content.IContent))]
[assembly: TypeForwardedTo(typeof(DioRed.Vermilion.Interaction.Content.TextContent))]
[assembly: TypeForwardedTo(typeof(DioRed.Vermilion.Interaction.Content.HtmlContent))]
[assembly: TypeForwardedTo(typeof(DioRed.Vermilion.Interaction.Content.ImageUrlContent))]
[assembly: TypeForwardedTo(typeof(DioRed.Vermilion.Interaction.Content.ImageBytesContent))]
[assembly: TypeForwardedTo(typeof(DioRed.Vermilion.Interaction.Content.ImageStreamContent))]
