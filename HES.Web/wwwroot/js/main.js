function createBreadcrumbs(breadcrumbs) {
    $('#breadcrumb').toggleClass('d-none');
    for (var i = 0; i < breadcrumbs.length; i++) {
        $('.breadcrumb').append('<li class="breadcrumb-item ' + breadcrumbs[i].active + '">' + breadcrumbs[i].content + '</li>');
    }
}

function showSpinner(elementId) {
    $('#' + elementId).toggleClass('d-none');
}

function toggleModalDialog(dialogId) {
    $('#' + dialogId).modal('toggle');
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