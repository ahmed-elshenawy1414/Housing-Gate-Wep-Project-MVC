$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

function Merge-Extra([string]$path, [hashtable]$extra) {
    $content = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
    $sb = New-Object System.Text.StringBuilder
    foreach ($key in ($extra.Keys | Sort-Object)) {
        $keyEsc = [System.Security.SecurityElement]::Escape($key)
        $valEsc = [System.Security.SecurityElement]::Escape($extra[$key])
        [void]$sb.AppendLine("  <data name=`"$keyEsc`" xml:space=`"preserve`">")
        [void]$sb.AppendLine("    <value>$valEsc</value>")
        [void]$sb.AppendLine("  </data>")
    }
    $content = $content.Replace("</root>", $sb.ToString() + "</root>")
    [System.IO.File]::WriteAllText($path, $content, $utf8NoBom)
    Write-Host "Re-merged $($extra.Count) extra keys into $path"
}

# Read the extra admin keys from the OLD resx that we saved before gen overwrote it
# Since we can't do that, we read from the SharedResource.resx which has the admin keys
# as part of the tsv-generated content... no, that's wrong.

# Instead, read the admin keys from the backup we can reconstruct.
# The extra keys are the ones that were in AdminAreaResource.resx before gen ran.
# We know the tsv has 102 admin keys. The old resx had 157 (102+55).
# The 55 extra keys are NOT in the tsv.

# Read the tsv to find which admin keys exist in tsv
$tsvPath = Join-Path $root "Resources\strings.tsv"
$tsvContent = [System.IO.File]::ReadAllText($tsvPath, [System.Text.Encoding]::UTF8)
$tsvAdminKeys = @{}
foreach ($line in $tsvContent.Split("`n")) {
    if ($line -match "^Areas/AdminAreaResource`t([^`t]+)`t([^`t]+)`t([^`t]+)$") {
        $tsvAdminKeys[$Matches[1]] = @{ en = $Matches[2]; ar = $Matches[3] }
    }
}

# The 55 extra keys we need to add (hardcoded from the previous session's work)
# These are the keys that were added to AdminAreaResource.resx manually
$extraKeys = @{}

# Common area
$extraKeys["Common.Back"] = @{ en = "Back"; ar = [char]0x0631 + [char]0x062C + [char]0x0648 + [char]0x0639 }
$extraKeys["Common.Cancel"] = @{ en = "Cancel"; ar = [char]0x0625 + [char]0x0644 + [char]0x063A + [char]0x0627 + [char]0x0621 }

# Complaint keys
$extraKeys["Cmpl.About"] = @{ en = "about"; ar = [char]0x0639 + [char]0x0646 }
$extraKeys["Cmpl.ColDate"] = @{ en = "Date"; ar = [char]0x0627 + [char]0x0644 + [char]0x062A + [char]0x0627 + [char]0x0631 + [char]0x064A + [char]0x062E }
$extraKeys["Cmpl.ColFrom"] = @{ en = "From"; ar = [char]0x0645 + [char]0x0646 }
$extraKeys["Cmpl.ColId"] = @{ en = "#"; ar = "#" }
$extraKeys["Cmpl.ColProperty"] = @{ en = "Property"; ar = [char]0x0627 + [char]0x0644 + [char]0x0639 + [char]0x0642 + [char]0x0627 + [char]0x0631 }
$extraKeys["Cmpl.ColStatus"] = @{ en = "Status"; ar = [char]0x0627 + [char]0x0644 + [char]0x062D + [char]0x0627 + [char]0x0644 + [char]0x0629 }
$extraKeys["Cmpl.ColType"] = @{ en = "Type"; ar = [char]0x0627 + [char]0x0644 + [char]0x0641 + [char]0x0626 + [char]0x0629 }
$extraKeys["Cmpl.Complaint"] = @{ en = "Complaint"; ar = [char]0x0634 + [char]0x0643 + [char]0x0648 + [char]0x0649 }
$extraKeys["Cmpl.Empty"] = @{ en = "No complaints filed."; ar = [char]0x0644 + [char]0x0627 + " " + [char]0x062A + [char]0x0648 + [char]0x062C + [char]0x062F + " " + [char]0x0634 + [char]0x0643 + [char]0x0627 + [char]0x0648 + [char]0x0649 + " " + [char]0x0645 + [char]0x0642 + [char]0x062F + [char]0x0645 + [char]0x0629 + "." }
$extraKeys["Cmpl.General"] = @{ en = "general"; ar = [char]0x0639 + [char]0x0627 + [char]0x0645 }
$extraKeys["Cmpl.Resolution"] = @{ en = "Resolution"; ar = [char]0x0627 + [char]0x0644 + [char]0x0642 + [char]0x0631 + [char]0x0627 + [char]0x0631 }
$extraKeys["Cmpl.Response"] = @{ en = "Response"; ar = [char]0x0627 + [char]0x0644 + [char]0x0631 + [char]0x062F }
$extraKeys["Cmpl.ResponseLabel"] = @{ en = "Response (optional, sent to the student)"; ar = [char]0x0627 + [char]0x0644 + [char]0x0631 + [char]0x062F + " (" + [char]0x0627 + [char]0x062E + [char]0x062A + [char]0x064A + [char]0x0627 + [char]0x0631 + [char]0x064A + [char]0x060C + " " + [char]0x064A + [char]0x0651 + [char]0x0631 + [char]0x0633 + [char]0x0644 + " " + [char]0x0644 + [char]0x0644 + [char]0x0637 + [char]0x0627 + [char]0x0644 + [char]0x0628 + ")" }
$extraKeys["Cmpl.Review"] = @{ en = "Review"; ar = [char]0x0645 + [char]0x0631 + [char]0x0627 + [char]0x062C + [char]0x0639 + [char]0x0629 }
$extraKeys["Cmpl.SaveResolution"] = @{ en = "Save resolution"; ar = [char]0x062D + [char]0x0641 + [char]0x0638 + " " + [char]0x0627 + [char]0x0644 + [char]0x0642 + [char]0x0631 + [char]0x0627 + [char]0x0631 }
$extraKeys["Cmpl.Title"] = @{ en = "Complaints"; ar = [char]0x0627 + [char]0x0644 + [char]0x0634 + [char]0x0643 + [char]0x0627 + [char]0x0648 + [char]0x0649 }

# Audit keys
$extraKeys["Audit.Title"] = @{ en = "Audit logs"; ar = [char]0x0633 + [char]0x062C + [char]0x0644 + [char]0x0627 + [char]0x062A + " " + [char]0x0627 + [char]0x0644 + [char]0x062A + [char]0x062F + [char]0x0642 + [char]0x064A + [char]0x0642 }
$extraKeys["Audit.Empty"] = @{ en = "No audit entries found."; ar = [char]0x0644 + [char]0x0627 + " " + [char]0x062A + [char]0x0648 + [char]0x062C + [char]0x062F + " " + [char]0x0633 + [char]0x062C + [char]0x0644 + [char]0x0627 + [char]0x062A + " " + [char]0x062A + [char]0x062F + [char]0x0642 + [char]0x064A + [char]0x0642 + "." }
$extraKeys["Audit.SearchPlaceholder"] = @{ en = "Search by action, entity, description or admin..."; ar = [char]0x0627 + [char]0x0628 + [char]0x062D + [char]0x062B + " " + [char]0x0628 + [char]0x0627 + [char]0x0644 + [char]0x062D + [char]0x062F + [char]0x062B + [char]0x060C + " " + [char]0x0627 + [char]0x0644 + [char]0x0643 + [char]0x064A + [char]0x0627 + [char]0x0646 + [char]0x060C + " " + [char]0x0627 + [char]0x0644 + [char]0x0648 + [char]0x0635 + [char]0x0641 + " " + [char]0x0623 + [char]0x0648 + " " + [char]0x0627 + [char]0x0644 + [char]0x0645 + [char]0x0633 + [char]0x0624 + [char]0x0648 + [char]0x0644 + "..." }
$extraKeys["Audit.AllActions"] = @{ en = "All actions"; ar = [char]0x062C + [char]0x0645 + [char]0x064A + [char]0x0639 + " " + [char]0x0627 + [char]0x0644 + [char]0x0623 + [char]0x062D + [char]0x062F + [char]0x062B }
$extraKeys["Audit.Filter"] = @{ en = "Filter"; ar = [char]0x062A + [char]0x0635 + [char]0x0641 + [char]0x064A + [char]0x0629 }
$extraKeys["Audit.ColTime"] = @{ en = "Time"; ar = [char]0x0627 + [char]0x0644 + [char]0x0648 + [char]0x0642 + [char]0x062A }
$extraKeys["Audit.ColAdmin"] = @{ en = "Admin"; ar = [char]0x0627 + [char]0x0644 + [char]0x0645 + [char]0x0633 + [char]0x0624 + [char]0x0648 + [char]0x0644 }
$extraKeys["Audit.ColAction"] = @{ en = "Action"; ar = [char]0x0627 + [char]0x0644 + [char]0x062D + [char]0x062F + [char]0x062B }
$extraKeys["Audit.ColEntity"] = @{ en = "Entity"; ar = [char]0x0627 + [char]0x0644 + [char]0x0643 + [char]0x064A + [char]0x0627 + [char]0x0646 }
$extraKeys["Audit.ColDetails"] = @{ en = "Details"; ar = [char]0x0627 + [char]0x0644 + [char]0x062A + [char]0x0641 + [char]0x0627 + [char]0x0635 + [char]0x064A + [char]0x0644 }
$extraKeys["Audit.ColReason"] = @{ en = "Reason"; ar = [char]0x0627 + [char]0x0644 + [char]0x0633 + [char]0x0628 + [char]0x0628 }
$extraKeys["Audit.ColIp"] = @{ en = "IP address"; ar = [char]0x0639 + [char]0x0646 + [char]0x0648 + [char]0x0627 + [char]0x0646 + " IP" }

# Dashboard keys
$extraKeys["Dash.ApprovedProperties"] = @{ en = "Approved listings"; ar = [char]0x0627 + [char]0x0644 + [char]0x0639 + [char]0x0642 + [char]0x0627 + [char]0x0631 + [char]0x0627 + [char]0x062A + " " + [char]0x0627 + [char]0x0644 + [char]0x0645 + [char]0x0639 + [char]0x062A + [char]0x0645 + [char]0x062F + [char]0x0629 }
$extraKeys["Dash.Dashboard"] = @{ en = "Dashboard"; ar = [char]0x0644 + [char]0x0648 + [char]0x062D + [char]0x0629 + " " + [char]0x0627 + [char]0x0644 + [char]0x062A + [char]0x062D + [char]0x0643 + [char]0x0645 }
$extraKeys["Dash.NoListings"] = @{ en = "No listings yet."; ar = [char]0x0644 + [char]0x0627 + " " + [char]0x062A + [char]0x0648 + [char]0x062C + [char]0x062F + " " + [char]0x0639 + [char]0x0642 + [char]0x0627 + [char]0x0631 + [char]0x0627 + [char]0x062A + " " + [char]0x0628 + [char]0x0639 + [char]0x062F + "." }
$extraKeys["Dash.NoPendingVerifications"] = @{ en = "No pending verification requests."; ar = [char]0x0644 + [char]0x0627 + " " + [char]0x062A + [char]0x0648 + [char]0x062C + [char]0x062F + " " + [char]0x0637 + [char]0x0644 + [char]0x0628 + [char]0x0627 + [char]0x062A + " " + [char]0x062A + [char]0x0648 + [char]0x062B + [char]0x064A + [char]0x0642 + " " + [char]0x0645 + [char]0x0639 + [char]0x0644 + [char]0x0642 + [char]0x0629 + "." }
$extraKeys["Dash.OwnersAwaiting"] = @{ en = "Owners awaiting verification"; ar = [char]0x0645 + [char]0x0627 + [char]0x0644 + [char]0x0643 + [char]0x0648 + [char]0x0646 + " " + [char]0x0628 + [char]0x0627 + [char]0x0646 + [char]0x062A + [char]0x0638 + [char]0x0627 + [char]0x0631 + " " + [char]0x0627 + [char]0x0644 + [char]0x062A + [char]0x0648 + [char]0x062B + [char]0x064A + [char]0x0642 }
$extraKeys["Dash.PendingProperties"] = @{ en = "Pending listings"; ar = [char]0x0627 + [char]0x0644 + [char]0x0639 + [char]0x0642 + [char]0x0627 + [char]0x0631 + [char]0x0627 + [char]0x062A + " " + [char]0x0627 + [char]0x0644 + [char]0x0645 + [char]0x0639 + [char]0x0644 + [char]0x0642 + [char]0x0629 }
$extraKeys["Dash.RecentListings"] = @{ en = "Recent listings"; ar = [char]0x0622 + [char]0x062E + [char]0x0631 + " " + [char]0x0627 + [char]0x0644 + [char]0x0639 + [char]0x0642 + [char]0x0627 + [char]0x0631 + [char]0x0627 + [char]0x062A }
$extraKeys["Dash.StudentsAwaiting"] = @{ en = "Students awaiting verification"; ar = [char]0x0637 + [char]0x0644 + [char]0x0627 + [char]0x0628 + " " + [char]0x0628 + [char]0x0627 + [char]0x0646 + [char]0x062A + [char]0x0638 + [char]0x0627 + [char]0x0631 + " " + [char]0x0627 + [char]0x0644 + [char]0x062A + [char]0x0648 + [char]0x062B + [char]0x064A + [char]0x0642 }
$extraKeys["Dash.Title"] = @{ en = "Admin dashboard"; ar = [char]0x0644 + [char]0x0648 + [char]0x062D + [char]0x0629 + " " + [char]0x062A + [char]0x062D + [char]0x0643 + [char]0x0645 + " " + [char]0x0627 + [char]0x0644 + [char]0x0625 + [char]0x062F + [char]0x0627 + [char]0x0631 + [char]0x0629 }
$extraKeys["Dash.TotalListings"] = @{ en = "Live listings"; ar = [char]0x0627 + [char]0x0644 + [char]0x0639 + [char]0x0642 + [char]0x0627 + [char]0x0631 + [char]0x0627 + [char]0x062A + " " + [char]0x0627 + [char]0x0644 + [char]0x0645 + [char]0x0646 + [char]0x0634 + [char]0x0648 + [char]0x0631 + [char]0x0629 }
$extraKeys["Dash.TotalUsers"] = @{ en = "Total users"; ar = [char]0x0625 + [char]0x062C + [char]0x0645 + [char]0x0627 + [char]0x0644 + [char]0x064A + " " + [char]0x0627 + [char]0x0644 + [char]0x0645 + [char]0x0633 + [char]0x062A + [char]0x062E + [char]0x062F + [char]0x0645 + [char]0x064A + [char]0x0646 }
$extraKeys["Dash.VerificationRequests"] = @{ en = "Verification requests"; ar = [char]0x0637 + [char]0x0644 + [char]0x0628 + [char]0x0627 + [char]0x062A + " " + [char]0x0627 + [char]0x0644 + [char]0x062A + [char]0x0648 + [char]0x062B + [char]0x064A + [char]0x0642 }
$extraKeys["Dash.ViewAll"] = @{ en = "View all"; ar = [char]0x0639 + [char]0x0631 + [char]0x0636 + " " + [char]0x0627 + [char]0x0644 + [char]0x0643 + [char]0x0644 }

# Property keys
$extraKeys["Props.All"] = @{ en = "All"; ar = [char]0x0627 + [char]0x0644 + [char]0x0643 + [char]0x0644 }
$extraKeys["Props.Approve"] = @{ en = "Approve"; ar = [char]0x0645 + [char]0x0648 + [char]0x0627 + [char]0x0641 + [char]0x0642 + [char]0x0629 }
$extraKeys["Props.Approved"] = @{ en = "Approved"; ar = [char]0x0645 + [char]0x0639 + [char]0x062A + [char]0x0645 + [char]0x062F }
$extraKeys["Props.ColLocation"] = @{ en = "Location"; ar = [char]0x0627 + [char]0x0644 + [char]0x0645 + [char]0x0648 + [char]0x0642 + [char]0x0639 }
$extraKeys["Props.ColOwner"] = @{ en = "Owner"; ar = [char]0x0627 + [char]0x0644 + [char]0x0645 + [char]0x0627 + [char]0x0644 + [char]0x0643 }
$extraKeys["Props.ColPublished"] = @{ en = "Published"; ar = [char]0x062A + [char]0x0627 + [char]0x0631 + [char]0x064A + [char]0x062E + " " + [char]0x0627 + [char]0x0644 + [char]0x0646 + [char]0x0634 + [char]0x0631 }
$extraKeys["Props.ColRooms"] = @{ en = "Rooms"; ar = [char]0x0627 + [char]0x0644 + [char]0x063A + [char]0x0631 + [char]0x0641 }
$extraKeys["Props.ColStatus"] = @{ en = "Status"; ar = [char]0x0627 + [char]0x0644 + [char]0x062D + [char]0x0627 + [char]0x0644 + [char]0x0629 }
$extraKeys["Props.ColTitle"] = @{ en = "Title"; ar = [char]0x0627 + [char]0x0644 + [char]0x0639 + [char]0x0646 + [char]0x0648 + [char]0x0627 + [char]0x0646 }
$extraKeys["Props.Hidden"] = @{ en = "Hidden"; ar = [char]0x0645 + [char]0x062E + [char]0x0641 + [char]0x064A }
$extraKeys["Props.Pending"] = @{ en = "Pending"; ar = [char]0x0642 + [char]0x064A + [char]0x062F + " " + [char]0x0627 + [char]0x0644 + [char]0x0627 + [char]0x0646 + [char]0x062A + [char]0x0638 + [char]0x0627 + [char]0x0631 }
$extraKeys["Props.ReasonLabel"] = @{ en = "Reason (shown to the owner)"; ar = [char]0x0627 + [char]0x0644 + [char]0x0633 + [char]0x0628 + [char]0x0628 + " (" + [char]0x0633 + [char]0x064A + [char]0x0638 + [char]0x0647 + [char]0x0631 + " " + [char]0x0644 + [char]0x0644 + [char]0x0645 + [char]0x0627 + [char]0x0644 + [char]0x0643 + ")" }
$extraKeys["Props.Reject"] = @{ en = "Reject"; ar = [char]0x0631 + [char]0x0641 + [char]0x0636 }
$extraKeys["Props.Rejected"] = @{ en = "Rejected"; ar = [char]0x0645 + [char]0x0631 + [char]0x0641 + [char]0x0648 + [char]0x0636 }
$extraKeys["Props.RejectListing"] = @{ en = "Reject listing"; ar = [char]0x0631 + [char]0x0641 + [char]0x0636 + " " + [char]0x0627 + [char]0x0644 + [char]0x0625 + [char]0x0639 + [char]0x0644 + [char]0x0627 + [char]0x0646 }
$extraKeys["Props.Title"] = @{ en = "Properties"; ar = [char]0x0627 + [char]0x0644 + [char]0x0639 + [char]0x0642 + [char]0x0627 + [char]0x0631 + [char]0x0627 + [char]0x062A }

# Review keys
$extraKeys["Rev.AllReviews"] = @{ en = "All reviews"; ar = [char]0x062C + [char]0x0645 + [char]0x064A + [char]0x0639 + " " + [char]0x0627 + [char]0x0644 + [char]0x062A + [char]0x0642 + [char]0x064A + [char]0x064A + [char]0x0645 + [char]0x0627 + [char]0x062A }
$extraKeys["Rev.Approve"] = @{ en = "Approve"; ar = [char]0x0645 + [char]0x0648 + [char]0x0627 + [char]0x0641 + [char]0x0642 + [char]0x0629 }
$extraKeys["Rev.AwaitingModeration"] = @{ en = "Awaiting moderation"; ar = [char]0x0628 + [char]0x0627 + [char]0x0646 + [char]0x062A + [char]0x0638 + [char]0x0627 + [char]0x0631 + " " + [char]0x0627 + [char]0x0644 + [char]0x0645 + [char]0x0631 + [char]0x0627 + [char]0x062C + [char]0x0639 + [char]0x0629 }
$extraKeys["Rev.ColProperty"] = @{ en = "Property"; ar = [char]0x0627 + [char]0x0644 + [char]0x0639 + [char]0x0642 + [char]0x0627 + [char]0x0631 }
$extraKeys["Rev.ColRating"] = @{ en = "Rating"; ar = [char]0x0627 + [char]0x0644 + [char]0x062A + [char]0x0642 + [char]0x064A + [char]0x064A + [char]0x0645 }
$extraKeys["Rev.ColReview"] = @{ en = "Review"; ar = [char]0x0627 + [char]0x0644 + [char]0x062A + [char]0x0639 + [char]0x0644 + [char]0x064A + [char]0x0642 }
$extraKeys["Rev.ColStatus"] = @{ en = "Status"; ar = [char]0x0627 + [char]0x0644 + [char]0x062D + [char]0x0627 + [char]0x0644 + [char]0x0629 }
$extraKeys["Rev.ColStudent"] = @{ en = "Student"; ar = [char]0x0627 + [char]0x0644 + [char]0x0637 + [char]0x0627 + [char]0x0644 + [char]0x0628 }
$extraKeys["Rev.NoPublished"] = @{ en = "No published reviews."; ar = [char]0x0644 + [char]0x0627 + " " + [char]0x062A + [char]0x0648 + [char]0x062C + [char]0x062F + " " + [char]0x062A + [char]0x0642 + [char]0x064A + [char]0x064A + [char]0x0645 + [char]0x0627 + [char]0x062A + " " + [char]0x0645 + [char]0x0646 + [char]0x0634 + [char]0x0648 + [char]0x0631 + [char]0x0629 + "." }
$extraKeys["Rev.PendingCount"] = @{ en = "{0} pending"; ar = "{0} " + [char]0x0628 + [char]0x0627 + [char]0x0646 + [char]0x062A + [char]0x0638 + [char]0x0627 + [char]0x0631 + " " + [char]0x0627 + [char]0x0644 + [char]0x0645 + [char]0x0631 + [char]0x0627 + [char]0x062C + [char]0x0639 + [char]0x0629 }
$extraKeys["Rev.Remove"] = @{ en = "Remove"; ar = [char]0x0625 + [char]0x0632 + [char]0x0627 + [char]0x0644 + [char]0x0629 }
$extraKeys["Rev.RemoveConfirm"] = @{ en = "Remove this review?"; ar = [char]0x0625 + [char]0x0632 + [char]0x0627 + [char]0x0644 + [char]0x0629 + " " + [char]0x0647 + [char]0x0630 + [char]0x0627 + " " + [char]0x0627 + [char]0x0644 + [char]0x062A + [char]0x0642 + [char]0x064A + [char]0x064A + [char]0x0645 + [char]0x061F }
$extraKeys["Rev.Title"] = @{ en = "Reviews"; ar = [char]0x0627 + [char]0x0644 + [char]0x062A + [char]0x0642 + [char]0x064A + [char]0x064A + [char]0x0645 + [char]0x0627 + [char]0x062A }

# Users keys
$extraKeys["Users.Activate"] = @{ en = "Activate"; ar = [char]0x062A + [char]0x0641 + [char]0x0639 + [char]0x064A + [char]0x0644 }
$extraKeys["Users.ActivateConfirm"] = @{ en = "Activate this user?"; ar = [char]0x062A + [char]0x0641 + [char]0x0639 + [char]0x064A + [char]0x0644 + " " + [char]0x0647 + [char]0x0630 + [char]0x0627 + " " + [char]0x0627 + [char]0x0644 + [char]0x0645 + [char]0x0633 + [char]0x062A + [char]0x062E + [char]0x062F + [char]0x0645 + [char]0x061F }
$extraKeys["Users.All"] = @{ en = "All"; ar = [char]0x0627 + [char]0x0644 + [char]0x0643 + [char]0x0644 }
$extraKeys["Users.AllUsers"] = @{ en = "All users"; ar = [char]0x062C + [char]0x0645 + [char]0x064A + [char]0x0639 + " " + [char]0x0627 + [char]0x0644 + [char]0x0645 + [char]0x0633 + [char]0x062A + [char]0x062E + [char]0x062F + [char]0x0645 + [char]0x064A + [char]0x0646 }
$extraKeys["Users.Approve"] = @{ en = "Approve"; ar = [char]0x0645 + [char]0x0648 + [char]0x0627 + [char]0x0641 + [char]0x0642 + [char]0x0629 }
$extraKeys["Users.ColActive"] = @{ en = "Active"; ar = [char]0x0646 + [char]0x0634 + [char]0x0637 }
$extraKeys["Users.ColCompany"] = @{ en = "Company"; ar = [char]0x0627 + [char]0x0644 + [char]0x0634 + [char]0x0631 + [char]0x0643 + [char]0x0629 }
$extraKeys["Users.ColDocument"] = @{ en = "Document"; ar = [char]0x0627 + [char]0x0644 + [char]0x0645 + [char]0x0633 + [char]0x062A + [char]0x0646 + [char]0x062F }
$extraKeys["Users.ColEmail"] = @{ en = "Email"; ar = [char]0x0627 + [char]0x0644 + [char]0x0628 + [char]0x0631 + [char]0x064A + [char]0x062F + " " + [char]0x0627 + [char]0x0644 + [char]0x0625 + [char]0x0644 + [char]0x0643 + [char]0x062A + [char]0x0631 + [char]0x0648 + [char]0x0646 + [char]0x064A }
$extraKeys["Users.ColJoined"] = @{ en = "Joined"; ar = [char]0x062A + [char]0x0627 + [char]0x0631 + [char]0x064A + [char]0x062E + " " + [char]0x0627 + [char]0x0644 + [char]0x0627 + [char]0x0646 + [char]0x0636 + [char]0x0645 + [char]0x0627 + [char]0x0645 }
$extraKeys["Users.ColName"] = @{ en = "Name"; ar = [char]0x0627 + [char]0x0644 + [char]0x0627 + [char]0x0633 + [char]0x0645 }
$extraKeys["Users.ColRoles"] = @{ en = "Roles"; ar = [char]0x0627 + [char]0x0644 + [char]0x0623 + [char]0x062F + [char]0x0648 + [char]0x0627 + [char]0x0631 }
$extraKeys["Users.ColStatus"] = @{ en = "Status"; ar = [char]0x0627 + [char]0x0644 + [char]0x062D + [char]0x0627 + [char]0x0644 + [char]0x0629 }
$extraKeys["Users.ColUniversity"] = @{ en = "University"; ar = [char]0x0627 + [char]0x0644 + [char]0x062C + [char]0x0627 + [char]0x0645 + [char]0x0639 + [char]0x0629 }
$extraKeys["Users.Deactivate"] = @{ en = "Deactivate"; ar = [char]0x0625 + [char]0x064A + [char]0x0642 + [char]0x0627 + [char]0x0641 }
$extraKeys["Users.DeactivateConfirm"] = @{ en = "Deactivate this user?"; ar = [char]0x0625 + [char]0x064A + [char]0x0642 + [char]0x0627 + [char]0x0641 + " " + [char]0x0647 + [char]0x0630 + [char]0x0627 + " " + [char]0x0627 + [char]0x0644 + [char]0x0645 + [char]0x0633 + [char]0x062A + [char]0x062E + [char]0x062F + [char]0x0645 + [char]0x061F }
$extraKeys["Users.NationalId"] = @{ en = "National ID"; ar = [char]0x0627 + [char]0x0644 + [char]0x0628 + [char]0x0637 + [char]0x0627 + [char]0x0642 + [char]0x0629 + " " + [char]0x0627 + [char]0x0644 + [char]0x0634 + [char]0x062E + [char]0x0635 + [char]0x064A + [char]0x0629 }
$extraKeys["Users.No"] = @{ en = "No"; ar = [char]0x0644 + [char]0x0627 }
$extraKeys["Users.Owners"] = @{ en = "Owners"; ar = [char]0x0627 + [char]0x0644 + [char]0x0645 + [char]0x0627 + [char]0x0644 + [char]0x0643 + [char]0x0648 + [char]0x0646 }
$extraKeys["Users.OwnerVerification"] = @{ en = "Owner verification"; ar = [char]0x062A + [char]0x0648 + [char]0x062B + [char]0x064A + [char]0x0642 + " " + [char]0x0627 + [char]0x0644 + [char]0x0645 + [char]0x0627 + [char]0x0644 + [char]0x0643 + [char]0x064A + [char]0x0646 }
$extraKeys["Users.ReasonForNew"] = @{ en = "Reason for new documents"; ar = [char]0x0633 + [char]0x0628 + [char]0x0628 + " " + [char]0x0637 + [char]0x0644 + [char]0x0628 + " " + [char]0x0645 + [char]0x0633 + [char]0x062A + [char]0x0646 + [char]0x062F + [char]0x0627 + [char]0x062A + " " + [char]0x062C + [char]0x062F + [char]0x064A + [char]0x062F + [char]0x0629 }
$extraKeys["Users.Reject"] = @{ en = "Reject"; ar = [char]0x0631 + [char]0x0641 + [char]0x0636 }
$extraKeys["Users.RejectionReason"] = @{ en = "Rejection reason"; ar = [char]0x0633 + [char]0x0628 + [char]0x0628 + " " + [char]0x0627 + [char]0x0644 + [char]0x0631 + [char]0x0641 + [char]0x0636 }
$extraKeys["Users.RequestNew"] = @{ en = "Request new"; ar = [char]0x0637 + [char]0x0644 + [char]0x0628 + " " + [char]0x0645 + [char]0x0633 + [char]0x062A + [char]0x0646 + [char]0x062F + [char]0x0627 + [char]0x062A + " " + [char]0x062C + [char]0x062F + [char]0x064A + [char]0x062F + [char]0x0629 }
$extraKeys["Users.Students"] = @{ en = "Students"; ar = [char]0x0627 + [char]0x0644 + [char]0x0637 + [char]0x0644 + [char]0x0627 + [char]0x0628 }
$extraKeys["Users.StudentVerification"] = @{ en = "Student verification"; ar = [char]0x062A + [char]0x0648 + [char]0x062B + [char]0x064A + [char]0x0642 + " " + [char]0x0627 + [char]0x0644 + [char]0x0637 + [char]0x0644 + [char]0x0627 + [char]0x0628 }
$extraKeys["Users.Title"] = @{ en = "Users"; ar = [char]0x0627 + [char]0x0644 + [char]0x0645 + [char]0x0633 + [char]0x062A + [char]0x062E + [char]0x062F + [char]0x0645 + [char]0x0648 + [char]0x0646 }
$extraKeys["Users.UniversityId"] = @{ en = "University ID"; ar = [char]0x0628 + [char]0x0637 + [char]0x0627 + [char]0x0642 + [char]0x0629 + " " + [char]0x0627 + [char]0x0644 + [char]0x062C + [char]0x0627 + [char]0x0645 + [char]0x0639 + [char]0x0629 }
$extraKeys["Users.View"] = @{ en = "View"; ar = [char]0x0639 + [char]0x0631 + [char]0x0636 }
$extraKeys["Users.Yes"] = @{ en = "Yes"; ar = [char]0x0646 + [char]0x0639 + [char]0x0645 }

# Build hashtables for merge - only include keys NOT in tsv
$enHash = @{}
$arHash = @{}
foreach ($k in $extraKeys.Keys) {
    if (-not $tsvAdminKeys.ContainsKey($k)) {
        $enHash[$k] = $extraKeys[$k].en
        $arHash[$k] = $extraKeys[$k].ar
    }
}
Write-Host "Found $($enHash.Count) extra admin keys (not in tsv)"

Merge-Extra (Join-Path $root "Resources\Areas\AdminAreaResource.resx") $enHash
Merge-Extra (Join-Path $root "Resources\Areas\AdminAreaResource.ar-EG.resx") $arHash

Write-Host "Done"
