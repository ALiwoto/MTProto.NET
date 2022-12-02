# Contributing to MTProto.NET Project

We're happy that you have chosen to contribute to the MTProto.NET project.

To organize the efforts made for this library, this simple guide is written in order to help you.

Please read this document completely before contributing to MTProto.NET.


## How To Contribute

MTProto.NET has a `Production` branch for stable releases and a `master` branch for daily development.  New features and fixes are always submitted to the `Production` branch.

If you are looking for ways to help, you should start by looking at the [Help Wanted tasks](https://github.com/ALiwoto/MTProto.NET/issues?q=is%3Aissue+is%3Aopen+label%3A%22Help+Wanted%22).  Please let us know if you plan to work on an issue so that others are not duplicating work.

The MTProto.NET project follows standard [GitHub flow](https://guides.github.com/introduction/flow/index.html).  You should learn and be familiar with how to [use Git](https://help.github.com/articles/set-up-git/), how to [create a fork of MTProto.NET](https://help.github.com/articles/fork-a-repo/), and how to [submit a Pull Request](https://help.github.com/articles/using-pull-requests/).

After you submit a PR, we will build your changes and verify all tests pass.  Project maintainers and contributors will review your changes and provide constructive feedback to improve your submission.

Once we are satisfied that your changes are good for MTProto.NET, we will merge it.


## Quick Guidelines

Here are a few simple rules and suggestions to remember when contributing to MTProto.NET.

* :bangbang: **NEVER** commit code that you didn't personally write.
* :bangbang: **NEVER** use decompiler tools to steal code and submit them as your own work.
* :bangbang: **NEVER** decompile another library's assemblies and steal another companies' copyrighted code.
* **PLEASE** try keep your PRs focused on a single topic and of a reasonable size or we may ask you to break it up.
* **PLEASE** be sure to write simple and descriptive commit messages.
* **DO NOT** surprise us with new APIs or big new features. Open an issue to discuss your ideas first.
* **DO NOT** reorder type members as it makes it difficult to compare code changes in a PR.
* **DO** try to follow our [coding style](CODESTYLE.md) for new code.
* **DO** give priority to the existing style of the file you're changing.
* **DO NOT** send PRs for code style changes or make code changes just for the sake of style.
* **PLEASE** keep a civil and respectful tone when discussing and reviewing contributions.
* **PLEASE** tell others about MTProto.NET and your contributions via social media.

## Code guidelines

Due to limitations on private target platforms, MTProto.NET enforces the use of C# 7.3 features.

It is however allowed to use the latest class library, but if contributions make use of classes which are not present in .NET 5.0, it will be required from the contribution to implement backward compatible switches.

These limitations should be lifted at some point.

## Licensing

The MTProto.NET library is under the [MIT License](https://opensource.org/licenses/MIT). 
See the [LICENSE](LICENSE) file for more details.  
Third-party libraries used by MTProto are under their own licenses.  Please refer to those libraries for details on the license they use.

We accept contributions in "good faith" that it isn't bound to a conflicting license.  By submitting a PR you agree to distribute your work under the MTProto license and copyright.

Thanks for reading this guide and helping make MTProto.NET great!
