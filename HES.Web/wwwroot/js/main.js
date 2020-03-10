function createBreadcrumbs(breadcrumbs) {
    $('#breadcrumb').toggleClass('d-none');
    for (var i = 0; i < breadcrumbs.length; i++) {
        if (breadcrumbs[i].active) {
            $('.breadcrumb').append('<li class="breadcrumb-item active">' + breadcrumbs[i].content + '</li>');
        }
        else {
            $('.breadcrumb').append('<li class="breadcrumb-item"><a href="' + breadcrumbs[i].link + '">' + breadcrumbs[i].content + '</a ></li>');
        }
    }
}

function showSpinner(elementId) {
    $('#' + elementId).toggleClass('d-none');
}

function toggleModalDialog(dialogId) {
    $('#' + dialogId).modal('toggle');
}

function triggerCheckbox(checkboxId) {
    var chkb = $('#' + checkboxId);
    chkb.trigger('click');
}

function toggleCheckbox(checkboxId) {
    var chkb = $('#' + checkboxId);
    chkb.attr("checked", !chkb.attr("checked"));
}

function toggleAllCheckboxes(tableId, state) {
    $('#' + tableId).find('input[type="checkbox"]').each(function () {
        $(this).attr('checked', state);
    });
}

// Logs Table
function initializeLogsTable() {
    $('#logs').DataTable({
        "paging": false,
        "info": false,
        "order": [[0, "desc"]]
    });
    var dataTable = $('#logs').dataTable();
    $('#searchbox').keyup(function () {
        dataTable.fnFilter(this.value);
    });
    $('.dataTables_filter').addClass('d-none');

    $('.content-body').scrollTop(0);
}

function destroyLogsTable() {
    $('#logs').DataTable().destroy();
}