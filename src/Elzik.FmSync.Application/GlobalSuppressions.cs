// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Style", 
    "IDE0042:Deconstruct variable declaration", 
    Justification = "I want to keep all variables concerned with logging together without needing to create a separate class to do so.", 
    Scope = "member", 
    Target = "~M:Elzik.FmSync.FrontMatterFileSynchroniser.SyncCreationDates(System.String)")]
