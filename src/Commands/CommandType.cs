namespace Zs.Bot.Services.Commands;

internal enum CommandType
{
    Default,

    ///<summary>SQL statement</summary>
    Sql, // TODO: rename to Db

    ///<summary>Command line interface</summary>
    Cli
}