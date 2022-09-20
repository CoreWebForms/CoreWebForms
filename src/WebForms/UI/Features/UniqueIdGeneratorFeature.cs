// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.UI.Features;

internal sealed class UniqueIdGeneratorFeature : IUniqueIdGeneratorFeature
{
    private const string _prefix = "ctl";
    private int _nextId;
    private readonly Control _owner;

    private static readonly string[] AutomaticIDs = new string[] {
            "ctl00", "ctl01", "ctl02", "ctl03", "ctl04", "ctl05", "ctl06",
            "ctl07", "ctl08", "ctl09", "ctl10", "ctl11", "ctl12", "ctl13",
            "ctl14", "ctl15", "ctl16", "ctl17", "ctl18", "ctl19", "ctl20",
            "ctl21", "ctl22", "ctl23", "ctl24", "ctl25", "ctl26", "ctl27",
            "ctl28", "ctl29", "ctl30", "ctl31", "ctl32", "ctl33", "ctl34",
            "ctl35", "ctl36", "ctl37", "ctl38", "ctl39", "ctl40", "ctl41",
            "ctl42", "ctl43", "ctl44", "ctl45", "ctl46", "ctl47", "ctl48",
            "ctl49", "ctl50", "ctl51", "ctl52", "ctl53", "ctl54", "ctl55",
            "ctl56", "ctl57", "ctl58", "ctl59", "ctl60", "ctl61", "ctl62",
            "ctl63", "ctl64", "ctl65", "ctl66", "ctl67", "ctl68", "ctl69",
            "ctl70", "ctl71", "ctl72", "ctl73", "ctl74", "ctl75", "ctl76",
            "ctl77", "ctl78", "ctl79", "ctl80", "ctl81", "ctl82", "ctl83",
            "ctl84", "ctl85", "ctl86", "ctl87", "ctl88", "ctl89", "ctl90",
            "ctl91", "ctl92", "ctl93", "ctl94", "ctl95", "ctl96", "ctl97",
            "ctl98", "ctl99",
            "ctl100", "ctl101", "ctl102", "ctl103", "ctl104", "ctl105", "ctl106",
            "ctl107", "ctl108", "ctl109", "ctl110", "ctl111", "ctl112", "ctl113",
            "ctl114", "ctl115", "ctl116", "ctl117", "ctl118", "ctl119", "ctl120",
            "ctl121", "ctl122", "ctl123", "ctl124", "ctl125", "ctl126", "ctl127"

        };

    public UniqueIdGeneratorFeature(Control owner)
    {
        _owner = owner;
    }

    public string? GetUniqueIdGenerator(Control control)
    {
        if (object.ReferenceEquals(_owner, control))
        {
            return control.ID;
        }

        var id = _nextId++;

        return id < AutomaticIDs.Length ? AutomaticIDs[id] : $"{_prefix}{id}";
    }
}
