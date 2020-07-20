/// <reference path="admin.vendor.js" />

var dateFormat = function () {
    var token = /d{1,4}|m{1,4}|yy(?:yy)?|([HhMsTt])\1?|[LloSZ]|"[^"]*"|'[^']*'/g,
        timezone = /\b(?:[PMCEA][SDP]T|(?:Pacific|Mountain|Central|Eastern|Atlantic) (?:Standard|Daylight|Prevailing) Time|(?:GMT|UTC)(?:[-+]\d{4})?)\b/g,
        timezoneClip = /[^-+\dA-Z]/g,
        pad = function (val, len) {
            val = String(val);
            len = len || 2;
            while (val.length < len) val = "0" + val;
            return val;
        };

    // Regexes and supporting functions are cached through closure
    return function (date, mask, utc) {
        var dF = dateFormat;

        // You can't provide utc if you skip other args (use the "UTC:" mask prefix)
        if (arguments.length == 1 && Object.prototype.toString.call(date) == "[object String]" && !/\d/.test(date)) {
            mask = date;
            date = undefined;
        }

        // Passing date through Date applies Date.parse, if necessary
        date = date ? new Date(date) : new Date;
        if (isNaN(date)) throw SyntaxError("invalid date");

        mask = String(dF.masks[mask] || mask || dF.masks["default"]);

        // Allow setting the utc argument via the mask
        if (mask.slice(0, 4) == "UTC:") {
            mask = mask.slice(4);
            utc = true;
        }

        var _ = utc ? "getUTC" : "get",
            d = date[_ + "Date"](),
            D = date[_ + "Day"](),
            m = date[_ + "Month"](),
            y = date[_ + "FullYear"](),
            H = date[_ + "Hours"](),
            M = date[_ + "Minutes"](),
            s = date[_ + "Seconds"](),
            L = date[_ + "Milliseconds"](),
            o = utc ? 0 : date.getTimezoneOffset(),
            flags = {
                d: d,
                dd: pad(d),
                ddd: dF.i18n.dayNames[D],
                dddd: dF.i18n.dayNames[D + 7],
                m: m + 1,
                mm: pad(m + 1),
                mmm: dF.i18n.monthNames[m],
                mmmm: dF.i18n.monthNames[m + 12],
                yy: String(y).slice(2),
                yyyy: y,
                h: H % 12 || 12,
                hh: pad(H % 12 || 12),
                H: H,
                HH: pad(H),
                M: M,
                MM: pad(M),
                s: s,
                ss: pad(s),
                l: pad(L, 3),
                L: pad(L > 99 ? Math.round(L / 10) : L),
                t: H < 12 ? "a" : "p",
                tt: H < 12 ? "am" : "pm",
                T: H < 12 ? "A" : "P",
                TT: H < 12 ? "AM" : "PM",
                Z: utc ? "UTC" : (String(date).match(timezone) || [""]).pop().replace(timezoneClip, ""),
                o: (o > 0 ? "-" : "+") + pad(Math.floor(Math.abs(o) / 60) * 100 + Math.abs(o) % 60, 4),
                S: ["th", "st", "nd", "rd"][d % 10 > 3 ? 0 : (d % 100 - d % 10 != 10) * d % 10]
            };

        return mask.replace(token, function ($0) {
            return $0 in flags ? flags[$0] : $0.slice(1, $0.length - 1);
        });
    };
}();

// Some common format strings
dateFormat.masks = {
    "default": "ddd mmm dd yyyy HH:MM:ss",
    shortDate: "m/d/yy",
    mediumDate: "mmm d, yyyy",
    longDate: "mmmm d, yyyy",
    fullDate: "dddd, mmmm d, yyyy",
    shortTime: "h:MM TT",
    mediumTime: "h:MM:ss TT",
    longTime: "h:MM:ss TT Z",
    isoDate: "yyyy-mm-dd",
    isoTime: "HH:MM:ss",
    isoDateTime: "yyyy-mm-dd'T'HH:MM:ss",
    isoUtcDateTime: "UTC:yyyy-mm-dd'T'HH:MM:ss'Z'"
};

// Internationalization strings
dateFormat.i18n = {
    dayNames: [
        "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat",
        "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
    ],
    monthNames: [
        "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
        "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"
    ]
};
// For convenience...
Date.prototype.format = function (mask, utc) {
    return dateFormat(this, mask, utc);
};



var datatable_server;
function ServerSideTable(dataServerSide) {
    if (dataServerSide.onloadSearch == undefined || dataServerSide.onloadSearch == true)
        loadServerData(dataServerSide);
    else {
        $(dataServerSide.searchSelector).bind("click", function () {
            $(dataServerSide.searchSelector).unbind();
            loadServerData(dataServerSide);
        });
    }
    function loadServerData(dataServerSide) {
        var searchValue = "";
        $.fn.dataTableExt.oApi.fnPagingInfo = function (oSettings) {
            return {
                "iStart": oSettings._iDisplayStart,
                "iEnd": oSettings.fnDisplayEnd(),
                "iLength": oSettings._iDisplayLength,
                "iTotal": oSettings.fnRecordsTotal(),
                "iFilteredTotal": oSettings.fnRecordsDisplay(),
                "iPage": Math.ceil(oSettings._iDisplayStart / oSettings._iDisplayLength),
                "iTotalPages": Math.ceil(oSettings.fnRecordsDisplay() / oSettings._iDisplayLength)
            };
        }
        datatable_server = $(dataServerSide.selector).dataTable({
            "fixedHeader": { header: true },
            "order": dataServerSide.order,
            "scrollX": true,
            "sDom": dataServerSide.sDom == undefined ? 'Tlfrtip' : dataServerSide.sDom,//'C<"clear">lfrtip',
            "stateSave": false,
            "colVis": dataServerSide.colVis,
            "processing": true,
            "oLanguage": {
                "sProcessing": "Please getting tre......"
            },
            //'fixedColumns': {
            //    leftColumns: dataServerSide.leftColumns == undefined ? -1 : dataServerSide.leftColumns,
            //    rightColumns: dataServerSide.rightColumns == undefined ? -1 : dataServerSide.rightColumns,
            //},
            "serverSide": true,
            "bFilter": false,
            "bAutoWidth": false,
            "pagingType": "full_numbers",
            "lengthMenu": [20, 40, 50, 75, 100],
            "pageLength": dataServerSide.pageLength == undefined ? 20 : dataServerSide.pageLength,
            "ajax": { "url": dataServerSide.url, "type": "POST" },
            "columns": dataServerSide.columns,
            "fnDrawCallback": function () {
                if (dataServerSide.callback)
                    dataServerSide.callback();
                setTimeout(function () {
                    var trIndex = 0;
                    $(dataServerSide.selector + ' tbody').find('tr').each(function () {
                        $(this).attr('data-index', trIndex);
                        trIndex++;
                    });
                }, 1000);
            },
            "fnServerParams": function (aoData) {
                var x = aoData;
                for (var i in aoData.columns) {
                    var ctrlName = '[name="search.' + aoData.columns[i].data + '"]';
                    if ($(ctrlName).length > 0) {
                        var search_value = "";
                        var search_filter_name = "";
                        $(ctrlName).each(function () {
                            var value = $(this).val().trim();
                            var required = $(this).data('required') != undefined;
                            if (value != '') {
                                search_value += "@#$" + value;
                                search_filter_name += "@#$" + ($(this).data('filter') == undefined ? "=" : $(this).data('filter'));
                            }
                            else if (required) {
                                search_value += "@#$" + randomString(10);
                                search_filter_name += "@#$" + ($(this).data('filter') == undefined ? "=" : $(this).data('filter'));
                            }
                        });
                        aoData.columns[i].search.value = search_value;
                        aoData.columns[i].name = search_filter_name;
                        aoData.columns[i].orderable = false;
                    }
                }
                aoData.search.value = searchValue;
            }
        });
        function randomString(length) {
            var chars='0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ';
            var result = '';
            for (var i = length; i > 0; --i) result += chars[Math.floor(Math.random() * chars.length)];
            console.log(result);
            return result;
        }
        $('html').on('click', dataServerSide.searchSelector, function (e) {
            e.preventDefault();
            datatable_server.fnDraw();

        });

        $(dataServerSide.resetSelector).on("click", function (e) {
            var $formContainer = $(".form-body").eq(0);
            $formContainer.find("[data-filter]").each(function () {
                if ($(this).is("select")) {
                    $(this).find("option:first").prop("selected", true);
                    //$(this).find("option").eq(0).attr('selected', 'selected');
                    if ($("select").eq(0).data("chosen") != undefined)
                        $(this).trigger("chosen:updated");
                    if ($("select").eq(0).hasClass("pluggin-selecttwo"))
                        if (typeof vendors !== "undefined") {
                            $("select").eq(0).find("option:first").prop("selected", true);
                            $("select").eq(0).select2("destroy");
                            vendors.initSelect2();
                        }
                }
                else if ($(this).is("input")) {
                    if ($(this).prop("type").toLowerCase() == "text") {
                        $(this).val("");
                    }
                }
            });
            $formContainer.find(":radio").each(function () {
                var name = $(this).attr("name");
                $("[name='search." + name + "']").val("");
                // $formContainer.find(":radio[name='" + name + "']").last().trigger("ifChange");
                $formContainer.find(":radio[name='" + name + "']").last().parents('label').click();


            });

            e.preventDefault();
            datatable_server.fnDraw();
        });


        if ($('.btnExportToExcel').length > 0) {
            $('html').on('click', '.btnExportToExcel', function (e) {
                e.preventDefault();
                searchValue = "Excel";
                datatable_server.fnDraw();
            });
        }
        if ($('.btnExportToPdf').length > 0) {
            $('html').on('click', '.btnExportToPdf', function (e) {
                e.preventDefault();
                searchValue = "Pdf";
                datatable_server.fnDraw();
            });
        }
        if ($('.btnExportToCSV').length > 0) {
            $('html').on('click', '.btnExportToCSV', function (e) {
                e.preventDefault();
                searchValue = "CSV";
                datatable_server.fnDraw();
            });
        }

        //export in formate
        //$("[data-export]").on("click", function (e) {
        //    e.preventDefault();
        //    var format = $(this).data('export');
        //    if (format != undefined && format !== "") {
        //        searchValue = format;
        //        datatable_server.fnDraw();
        //    }
        //});

        datatable_server.on('draw.dt', function (aoData) {

            //equal size buttons 
            // $('.btn-equal').css("width", Math.max.apply(Math, $('.btn-equal').map(function () { return $(this).width(); }).get()));

            if ($.fn.WaitProcess != undefined)
                $.fn.WaitProcess();
            if (RepairImages != undefined)
                RepairImages();
            if (searchValue != "") {
                searchValue = "";
                location.href = "/Admin/" + $("[name='_Controller_']").val() + "/DownloadReport"
                // location.href = "../../" + window.location.pathname + "/DownloadReport";
            }
            if ($.fn.WaitProcess != undefined)
                $.fn.HideWaitProcess();
            if ($.isFunction($.fn.bootstrapSwitch)) {
                if (typeof vendors !== "undefined")
                    vendors.initDynamicSwitchs();
            }
            if ($.isFunction($.fn.iCheck)) {
                if (typeof vendors !== "undefined")
                    vendors.initICheck();
            }
            if (typeof transactionActions !== 'undefined')
                transactionActions();

        });
        nestedTable();
        initRowSelection();

        if (dataServerSide.localSearch) {
            var ca = $('.create-action');
            if (ca.length != 0) {
                ca.prepend('<input type="text" class="local-search-grid form-control" placeholder="search in loaded results" />');
            }
        }

        $(".local-search-grid").on('change keyup paste', function () {
            searchGrid($(this).val());
        });

        function searchGrid(keyword) {
            var $list = $(dataServerSide.selector + ' tbody').find('tr');

            $list.sort(function (a, b) {
                if (keyword == "")
                    return $(a).data('index') > $(b).data('index');
                else
                    return $(a).text().indexOf(keyword) < $(b).text().indexOf(keyword);
            });

            $list.detach().appendTo($(dataServerSide.selector + ' tbody'));
            $list.each(function () {
                if ($(this).text().indexOf(keyword) != -1 && keyword != "")
                    $(this).removeClass('not-found').addClass('found');
                else
                    $(this).removeClass('found').addClass('not-found');
            });

            $(dataServerSide.selector + ' tbody tr:odd').removeClass('even').addClass('odd');
            $(dataServerSide.selector + ' tbody tr:even').removeClass('odd').addClass('even');
        }
    }
}


function removeRowServerTable() {
    var page_number = datatable_server.fnPagingInfo().iPage;
    var href = $(document).data("delete-url");
    row = $($('a[href="' + href + '"]').parents('tr')).get(0);
    datatable_server.fnDeleteRow(datatable_server.fnGetPosition(row), function () { datatable_server.fnPageChange(page_number); }, false);
    $(document).removeData("delete-url");
}

function ConvertToDate(data, dateFormat) {
    if (data == null) return '1/1/1950';
    var r = /\/Date\(([0-9]+)\)\//gi
    var matches = data.match(r);
    if (matches == null) return '1/1/1950';
    var result = matches.toString().substring(6, 19);
    var epochMilliseconds = result.replace(
    /^\/Date\(([0-9]+)([+-][0-9]{4})?\)\/$/,
    '$1');
    var b = new Date(parseInt(epochMilliseconds));
    if (!dateFormat)
        dateFormat = 'dd/mmm/yyyy hh:MM:ss TT';

    return b.format("dd/mm/yyyy HH:MM");

    var c = new Date(b.toString());
    var curr_date = c.getDate();
    var curr_month = c.getMonth() + 1;
    var curr_year = c.getFullYear();
    var curr_h = c.getHours();
    var curr_m = c.getMinutes();
    var curr_s = c.getSeconds();
    var curr_offset = c.getTimezoneOffset() / 60
    var d = curr_month.toString() + '/' + curr_date + '/' + curr_year + " " + curr_h + ':' + curr_m + ':' + curr_s;
    return d;
}
function ConvertToBoolean(bool, clickable, url) {
    var enable = '<i class="fa fa-thumbs-up" style="color: #10B610; font-size: 22px; " title="Enabled"></i>';
    var disable = '<i class="fa fa-thumbs-down" style="color: #D2312D; font-size: 22px;" title="Disabled"></i>';
    if (clickable) {
        enable = '<a href="' + url + '" class="btn-enabled">' + enable + '</a>';
        disable = '<a href="' + url + '" class="btn-enabled">' + disable + '</a>';
    }
    if (bool)
        return enable;
    else
        return disable;
}

function IOSwitchDynamic(bool, url, ontext, offtext, onMessage, offMessage, confirmDialogMsg) {

    offMessage = (onMessage != undefined && offMessage == undefined) ? onMessage : offMessage;
    var onMsg = onMessage == undefined || onMessage == "" ? '' : 'data-on-message="' + onMessage + '"';
    var offMsg = offMessage == undefined || offMessage == "" ? '' : 'data-off-message="' + offMessage + '"';
    var onText = ontext == undefined || ontext == "" ? 'data-on-label="Yes"' : 'data-on-label="' + ontext + '"';
    var offText = offtext == undefined || offtext == "" ? 'data-off-label="No"' : 'data-off-label="' + offtext + '"';
    var ischecked = bool == undefined ? false : bool;
    var isReadOnly = url == "";
    var dialogMsg = confirmDialogMsg == undefined || confirmDialogMsg == "" ? '' : 'data-confirm-message="' + confirmDialogMsg + '"';

    //var html = '<div data-on="success" ' + onText + " " + offText + '" data-off="danger" class="make-switch switch-mini">';
    var html = '<input ' + (isReadOnly ? 'disabled="disabled"' : '') + '  ' + (dialogMsg + " ") + (onText + " " + offText) + (onMsg == "" ? '' : onMsg) + '  ' + (offMsg == "" ? '' : offMsg) + '  data-url="' + url + '"  data-dynamic-switch type="checkbox"  class="switch" ' + (ischecked ? 'checked' : '') + ' />';
    //html += '</div>';
    return html;

}

function nestedTable() {
    function formatExpend(model) {
        var html = "";
        var transmoduleid = 0, transrefid = 0;
        if (Object.keys(model).length) {
            transrefid = model[Object.keys(model)[0]];
            if (model.hasOwnProperty("TransModuleID"))
                transmoduleid = model["TransModuleID"];
        }

        $.when($.ajax({
            url: "/Admin/TransactionLog/GetLogTable",
            data: { TransModuleID: transmoduleid, TransRefID: transrefid },
            method: "GET",
            success: function (jsonHtml) {
                var nTr = $(document).data("nTr");
                datatable_server.fnOpen(nTr, jsonHtml, 'details');
                $(document).removeData("nTr");
            }
        }));

        return html;

    }
    $('#datatable-server-side tbody td.details-control').live('click', function () {
        var nTr = $(this).parents('tr')[0];
        $(document).data("nTr", nTr);
        if (datatable_server.fnIsOpen(nTr)) {
            $(nTr).removeClass('shown');
            datatable_server.fnClose(nTr);
        } else {
            $(nTr).addClass('shown');
            datatable_server.fnOpen(nTr, '<div class="log-grid-loading">Loading Data...</div>', 'details');
            var trIndex = $('tr[role="row"]').index(nTr);
            formatExpend(datatable_server.fnGetData()[trIndex - 2]);
        }
    });

}

function initRowSelection() {
    var arrIDs = [];
    $("#datatable-server-side tbody").on("click", "td.row-selection input[type=\"checkbox\"]", function (e) {
        //var primarykey = "MoneyTransferID";
        var $row = $(this).closest("tr");
        //var rowData = datatable_server.fnGetData()[$row.index()];
        var id = $(this).data("id") == undefined ? 0 : $(this).data("id");
        var index = $.inArray(id, arrIDs);

        if (this.checked && index === -1) {
            arrIDs.push(id);
        } else if (!this.checked && index !== -1) {
            arrIDs.splice(index, 1);
        }

        if (this.checked) {
            $row.addClass("selected");
        } else {
            $row.removeClass("selected");
        }
        updateCheckboxState();
        console.log(arrIDs);
        e.stopPropagation();
    });
    $("input[name=\"selectrows\"]").on("click", function (e) {
        var $table = $("#datatable-server-side");
        if ($(this).prop("checked")) {
            $table.find("td.row-selection input[type=\"checkbox\"]:not(:checked)").trigger("click");
        } else {
            $table.find("td.row-selection input[type=\"checkbox\"]:checked").trigger("click");
        }
        e.stopPropagation();
    });
}

function updateCheckboxState() {
    //var $table = table.table().node();
    var $chkboxAll = $('td.row-selection input[type="checkbox"]');
    var $chkboxChecked = $('td.row-selection input[type="checkbox"]:checked');
    var chkboxSelectAll = $('input[name="selectrows"]');

    if ($chkboxChecked.length === 0) {
        $(chkboxSelectAll).prop("checked", false);
        $(chkboxSelectAll).prop("indeterminate", false);

    } else if ($chkboxChecked.length === $chkboxAll.length) {
        $(chkboxSelectAll).prop("checked", true);
        $(chkboxSelectAll).prop("indeterminate", false);
    } else {
        $(chkboxSelectAll).prop("checked", false);
        $(chkboxSelectAll).prop("indeterminate", true);
    }
}
function getSelectedRowIDs() {
    var arrIDs = [];
    $("td.row-selection input[type=\"checkbox\"]").each(function () {
        var id = $(this).data("id") == undefined ? 0 : $(this).data("id");
        var index = $.inArray(id, arrIDs);
        if (id > 0) {
            if (this.checked && index === -1) {
                arrIDs.push(id);
            } else if (!this.checked && index !== -1) {
                arrIDs.splice(index, 1);
            }
        }
    });
    return arrIDs;
}

