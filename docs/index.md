# Vermilion

Vermilion is a .NET chat-bot framework focused on simple setup and swappable implementations.

At runtime, a bot is built from three main pieces:

- exactly one chat storage
- one or more connectors
- a set of command handlers

Choose the path that fits your goal:

- Beginner path:
  use the complete `DioRed.Vermilion` package and the shortest startup syntax
- Detailed path:
  use `DioRed.Vermilion.Hosting` plus separate connector and storage packages
- Manual path:
  construct `BotCore` directly without `Hosting`

Start here:

- [Beginner Path](getting-started/beginner-path.md)
- [Detailed Path](getting-started/detailed-path.md)
- [Manual Path](getting-started/manual-path.md)

If you are upgrading from v14, see [v14 to v15](migration/v14-to-v15.md).
