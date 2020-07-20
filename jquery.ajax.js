(function ajaxInjection() {
    var _fnSetDependent = function ($select, trigger) {
        const options = $($select).data("pluggin-select2");
        if (options != undefined && options instanceof Object) {
            var control = options.control;
            var othercontrols = options.otherControls;
            var value = $($select).val();
            var id = $($select).attr('id');
            var postData = {};
            postData[id] = value == "" ? 0 : value;
            postData.DependendControlId = value == "" ? 0 : value;
            if (othercontrols != null && othercontrols != undefined && othercontrols.length > 0) {
                var ids = othercontrols.split(',');
                for (var i = 0; i < ids.length; i++) {
                    $(ids[i]).each(function () {
                        var ctrlId = $(this).attr('id');
                        postData[ctrlId] = $(this).val();
                        $(this).data("param", postData);
                    });
                }
            }

            if ($(control).length > 0) {
                $(control).data("param", postData);

                if (trigger) {
                    var otherControlIds = othercontrols.split(',');
                    for (var i = 0; i < otherControlIds.length; i++) {
                        $(otherControlIds[i]).each(function () {
                            var ctrlId = $(this).attr('id');
                            $(this).val('');
                            $(this).change();
                        });
                    }

                    $(control).val('');
                    $(control).html('');
                    $(control).change()
                }
            }
        }
    };
    var _oSelectors = { document: { contentwrapper: '.content-wrapper', contentTitle: '.box-header > .box-title', modal: '#application-modal', imageOrdering: '#image-order' } };

    this.fnHighlightMenu = function ($anchor) {
        if ($anchor && $anchor.length > 0 && $anchor.is('a')) {
            $('ul.sidebar-menu li.active').removeClass("active");
            $anchor.closest("li").addClass('active');
        }
    }

    this.fnAjaxView = function (url, wrapper, elementforshow, myfunction, header) {
        $(".mobPosDropdown").remove();
        $.ajax({
            url: url,
            method: 'get',
            dataType: 'html',
            headers: {
                'data': header
            },
            success: function (html) {
                var isMessageOnly = false;
                $('.modal-backdrop').fadeOut();
                if (!(html instanceof Object) && html.indexOf("status") != -1 && html.indexOf("alert") != -1) {
                    try {
                        html = JSON.parse(html);
                        if (html.alert !== null && html.alert.message != null) {
                            fnShowMessage(html.alert.message, html.alert.type);
                            isMessageOnly = true;
                        }
                        if (html.navigateUrl !== null && html.navigateUrl !== '') {
                            fnAjaxView(html.navigateUrl);
                            fnChangeUrl(html.navigateUrl);
                        }
                    } catch (e) {
                    }
                }
                if (!isMessageOnly) {
                    if (wrapper != undefined) {
                        $(wrapper).html(html);
                        if (elementforshow != null) {
                            $(elementforshow).show();
                        }
                    }
                    else {
                        $(_oSelectors.document.contentwrapper).html(html);
                    }
                    $('html, body').animate({
                        scrollTop: $(wrapper || _oSelectors.document.contentwrapper).offset().top
                    }, 500);

                    var _forms = $(wrapper || _oSelectors.document.contentwrapper).find("form").eq(0);
                    $(_forms).each(function () {
                        if ($.validator.unobtrusive != undefined) {
                            $.validator.unobtrusive.parse($(this));
                        }
                    });

                    fnInitAjaxComponents();
                    if (wrapper == undefined) {
                        fnChangeTab(window.location.hash);
                    }
                    if (typeof widgets !== 'undefined') {
                        widgets();
                    }
                    if ($(".box-title").html() != undefined) {
                        $(document).prop('title', $(".box-title").html() + " | " + applicationName);
                    }
                    excelImport();

                    if ($.fn.tooltip) {
                        $('[title]').attr('data-placement', 'auto').tooltip();
                    }
                    var area = window.location.pathname.split("/")[1].toLowerCase();
                    if (area == salesarea || area == bookingarea || area == accountarea || area == dashboard) {
                        $("[data-fy]").show();
                    }
                    else {
                        $("[data-fy]").hide();
                    }
                }
                if (typeof setTransportSheduleScroll == "function") {
                    setTransportSheduleScroll();
                }
                if ($.isFunction(myfunction)) {
                    myfunction();
                }
            },
            error: function (xhr, request, error) {
                var errors = JSON.parse(xhr.responseText);
                if (errors.url != undefined) {
                    if (errors.statuscode == "405") {
                        fnAjaxView(errors.url);
                        fnChangeUrl(errors.url);
                        swal("", errors.msg, "warning");
                    }
                    else if (errors.statuscode == "401") {
                        swal("", errors.msg, "error");
                        location = errors.url;
                    }
                }
            }
        });
    };

    this.fnChangeUrl = function (url) {
        localStorage.setItem("prevUrl", window.location.href);
        if (typeof (isPageUperTagRemove) != 'undefined') {
            delete isPageUperTagRemove;
        }
        if (typeof (isPageLock) != 'undefined') {
            delete isPageLock;
        }
        if (typeof (extensionremove) != 'undefined') {
            delete extensionremove;
        }
        document.title = $(_oSelectors.document.contentwrapper)
            .find(_oSelectors.document.contentTitle).html() || '' + " | " + applicationName;
        window.history.pushState({}, document.title, window.location.origin + url);
    }

    this.fnShowMessage = function (message, type, delay) {
        $.bootstrapGrowl(message || '', { type: (type || '').toLowerCase(), delay: delay || 5000 });
    }

    this.fnAppedHtml = function (html) {
        var formData = $(document).data('form-data');
        if (formData && formData.target && $(formData.target)) {
            if (formData.targettype && formData.targettype === 'tr') {
                $(formData.target).find('tr.empty-row').remove();
                if ($(formData.target).find('tr').length == 0)
                    $(formData.target).html(html);
                else {
                    if ($(html).is('tr') && $(html).attr('id') !== '' &&
                        $('tr[id="' + $(html).attr('id') + '"]').length > 0) {
                        $('tr[id="' + $(html).attr('id') + '"]').html($(html).html())
                    } else {
                        $(formData.target).find('tr:last').after($(html));
                    }
                }
            }
            else {
                $(formData.target).html(html);
            }
            if (formData.modal && formData.modal === 'close') {
                $(_oSelectors.document.modal).modal('hide');
            }
        }
    }

    this.fnPreview = function (input, target, $this) {
        var validspan = $('[data-validation-span="' + target + '"]');
        validspan.text('');
        if (input.files) {
            const $container = $('<div />', { class: 'box box-solid col-sm-12 no-padding' });
            const $header = $('<div />', {
                class: 'box-header with-border'
            }).append($('<i />', { class: 'fa fa-image' }))
                .append($('<h3 />', { class: 'box-title' }).html('Preview'));
            const $body = $('<div />', { class: 'box-body' });
            $(target).html('')

            isPreview = true;
            var max = $this.data('maxsize');
            var min = $this.data('minsize');
            if (max != undefined && min != undefined) {
                for (var i = 0; i < input.files.length; i++) {
                    var accept = $this.data('img-accept').toLowerCase();
                    var filename = input.files[0].name;
                    if (accept != undefined) {
                        var arrImgType = accept.split(',');
                        var ext = filename.substring(filename.lastIndexOf('.') + 1);
                        if ($.inArray(ext.toLowerCase(), arrImgType) < 0) {
                            validspan.text("Select file type only : " + accept);
                            $this.val('');
                            isPreview = false;
                            break
                        }
                    }

                    size = input.files[i].size;
                    if (input.files[i].size < min || input.files[i].size > max) {
                        validspan.text('file size accept min ' + filesize(min) + ' and max ' + filesize(max));
                        $this.val('');
                        isPreview = false;
                        break;
                    }
                }
            }

            if (isPreview) {
                validspan.text('');
                for (i = 0; i < input.files.length; i++) {
                    var reader = new FileReader();

                    reader.onload = function (event) {
                        $body.append($('<div />', { class: 'preview-thumbs col-md-1' })
                            .append($('<img />', { class: 'thumbnail', src: event.target.result }))
                        );
                        $header.appendTo($container);
                        $body.appendTo($container);
                        $container.appendTo(target);

                        $(target).removeClass('hide');
                    }
                    reader.readAsDataURL(input.files[i]);
                }
            }
        }
    }

    this.filesize = function (size) {
        var fSExt = new Array('Bytes', 'KB', 'MB', 'GB'),
            i = 0; while (size > 900) { size /= 1024; i++; }
        return exactSize = (Math.round(size * 100) / 100) + ' ' + fSExt[i];
    }

    this.fnChangeTab = function (tab) {
        var $tab = $('input[name="tabs"]:hidden');
        if ($tab.length == 0) {
            $tab = $('[nav-tabs-custom] input[name="tabs"]:hidden');
        }
        if ($tab.length > 0 && $tab.val().length > 0) {
            const active = (tab || "") === "" ? $tab.val() : tab;
            const target = $('a[href="' + active + '"]');
            if (target) {
                target.click();
            }
        }
    }
    this.fnModalClose = function () {
        $(_oSelectors.document.modal).modal("hide");
    }

    this.fnModal = function (title, url, formData, customParent) {
        var $modal = customParent != null ? $(customParent) : $(_oSelectors.document.modal);
        var isCustom = $(customParent).length > 0;
        if ($modal) {
            if (!isCustom) {
                $modal.find(".modal-title").html((title || ''));
                $modal.find('.modal-body').html(overlayTemplate);
                $modal.find(".modal-dialog").removeAttr("style");
                $modal.modal({ keyboard: false, backdrop: false }).show();
            }
            $.ajax({
                url: url,
                data: formData || {},
                method: 'get',
                dataType: 'html',
                success: function (html) {
                    if (isCustom) {
                        $modal.html(html);
                    }
                    else {
                        $(_oSelectors.document.modal).find('.modal-body')
                            .html(html);
                    }
                    fnInitValidator(true);
                    fnInitAjaxComponents(true);
                },
                error: function (xhr, request, error) {
                }
            });
        }
    };

    this.fnGetBadgesCount = function () {
        $('.badges-counter').each(function () {
            var url = $(this).data("url");
            if (url && url !== null && url !== '') {
                $.ajax({
                    url: url,
                    success: function (data) {
                        $('.badges-counter').html(data);
                    }
                })
            }
        });
    };

    this.fnDeleteMultipleImages = function () {
        $("li.image-select").click(function () {
            $(this).toggleClass("checked");
            var ids = [];
            var imgs = [];
            $('li.image-select.checked').each(function () {
                ids.push($(this).data("id") || "");
                imgs.push($(this).data("img") || "");
            });

            var ul = $(this).closest('ul');
            var target = ul.data('target');
            var $actionlink = $(target).find('.delete-multiimages')
            if (ids.length == 0) {
                $actionlink.remove();
            }
            if (ids.length > 0 && $(target).length > 0) {
                var url = $(ul).data('deleteurl');
                if ($actionlink.length == 0) {
                    var $action = $("<a />", {
                        class: 'btn btn-xs btn-danger pull-right delete-multiimages',
                    }).attr('href', url).attr('data-ids', ids.join(',')).attr('data-imgs', imgs.join(',')).html('Delete Images');

                    $action.appendTo(target);
                } else {
                    $actionlink.attr('href', url);
                    $actionlink.attr('data-ids', ids.join(',')).attr('data-imgs', imgs.join(','));
                }
            }
        })

        if ($(_oSelectors.document.imageOrdering).data('changeorder') == true) {
            $(_oSelectors.document.imageOrdering).sortable({
                stop: function (e, ui) {
                    var $image = $(ui.item[0]);
                    $(".ui-sortable-handle").each(function () {
                        $image = $(this);
                        var index = $image.index() + 1;
                        //send ajax requet to update order
                        ////$.ajax({
                        ////    url: '/admin/package/udpateorder',
                        ////    method: 'post',
                        ////    data: {
                        ////        id: $image.data("id") || '',
                        ////        index: index
                        ////    },
                        ////    success: function (data) {
                        ////    },
                        ////    error: function (xhr, request, error) {
                        ////    }
                        ////});
                    });
                }
            });
        }

        $("#image-order").disableSelection();
    };

    this.fnInitValidator = function (modal) {
        $((modal && modal == true)
            ? _oSelectors.document.modal
            : _oSelectors.document.contentwrapper).find("form").each(function () {
                if ($.validator.unobtrusive != undefined) {
                    $.validator.unobtrusive.parse($(this));
                }
                //if ($(this).data('reset-validator') == true) {
                //    var validator = $(this).data('validator');
                //    if (validator) {
                //        validator.resetForm();
                //        $(this).find('[data-valmsg-for]').html('');
                //    }

                //}
            });
    };
    this.getFyStartDate = function () {
        var $fy = $("[data-fy-startdate].active");
        var fysd;
        if ($fy.closest('li').data() != undefined) {
            fysd = $fy.closest('li').data().fyStartdate
        }
        return fysd;
    }
    this.getFyEndDate = function () {
        var $fy = $("[data-fy-enddate].active")
        var fyed;
        if ($fy.closest('li').data() != undefined) {
            fyed = $fy.closest('li').data().fyEnddate;
        }
        return fyed;
    }

    this.fnTruncateDate = function (date) {
        return new Date(date.getFullYear(), date.getMonth(), date.getDate());
    }
    this.fnInitiCheck = function (modal, parent) {
        var container = parent == undefined ? (modal && modal == true) ? _oSelectors.document.modal
            : _oSelectors.document.contentwrapper : parent;
        if ($.fn.iCheck != undefined) {
            $(container).find(".iCheck").each(function () {
                $(this).iCheck({
                    checkboxClass: 'icheckbox_square-blue',
                    radioClass: 'iradio_square-blue',
                    increaseArea: '20%'
                });
            });

            $(container).find("[data-checkall]").each(function () {
                var options = {};
                var opts = $(this).data("checkall");
                if (opts instanceof Object) {
                    options = opts;
                }
                $(this).checkAll(options);
            });
        }
    }
    this.fnInitSelect2 = function (modal, target) {
        var container = (modal && modal == true) ? _oSelectors.document.modal
            : _oSelectors.document.contentwrapper;

        if (target != undefined && $(target).length > 0) {
            container = target;
        }
        if ($.fn.select2 != undefined) {
            $(container).find("[data-pluggin-select2]").each(function () {
                var $element = $(this), options = {};
                //console.log($element.attr("id"));
                var oSettings = $element.data("pluggin-select2");
                if (oSettings) {
                    $.extend(oSettings, options);
                    $element.removeAttr("data-pluggin-select2");
                    $element.data("pluggin-select2", oSettings);
                }
                if (oSettings.disabled) {
                    $(this).before("<input type='hidden' name=" + $(this).attr("name") + " value='" + ($(this).val() != null ? $(this).val() : "") + "' />");
                }

                if (oSettings.url && oSettings.ajaxCall) {
                    var data = $(this).data("param");
                    oSettings.ajax = {
                        url: oSettings.url,
                        dataType: 'json',
                        delay: oSettings.delay,
                        data: function (params) {
                            var options = $(this).data("plugginSelect2") || {};
                            var addition = $(this).data("param") || {};
                            var params = {
                                search: params.term || "",
                                page: params.page || 1,
                                //ControlId: String(options.currentValue || '') || "",
                                DependendControlId: String(addition.DependendControlId || '') || "",
                                CompanyId: $('[name="route-companyid"]').val() || '',
                            };
                            if (($(this).data('params') || '').length > 0) {
                                var keys = ($(this).data('params')).split(',');
                                var depOtherControls = '';
                                for (var i = 0; i < keys.length; i++) {
                                    var $ele = $(keys[i]);
                                    if ($ele.length > 0) {
                                        var id = $ele.attr("id")
                                        var subId = "";
                                        if (id.indexOf("__") > 0) {
                                            subId = id.substr(id.indexOf("__") + 2);
                                        }
                                        else {
                                            subId = id;
                                        }

                                        depOtherControls += "&" + (subId || '') + "=" + ($ele.val() == null ? "" : $ele.val());
                                        params[(subId || '')] = $ele.val();
                                    }
                                }
                                params['dependendothercontrolid'] = depOtherControls;
                            }
                            return params;
                        },
                        processResults: function (data, params) {
                            params.page = params.page || 1;
                            return {
                                results: data.results || data.Result.results,
                                pagination: {
                                    more: (data.pagination || data.Result.pagination).more
                                }
                            };
                        },
                        cache: true
                    }

                    oSettings.escapeMarkup = function (markup) { return markup; };
                    oSettings.templateResult = function (oData) { return oData.text; };
                    oSettings.templateSelection = function (data) {

                        if (oSettings.otherInfo != null || $(data.element).parent().data("otherInfo") != undefined) {
                            var $remark = $('[data-otherinfo="' + $element.attr("name") + '"]');
                            if ($element.val() != null && $element.data().select2 != undefined) {
                                var additional = $element.select2('data')[0].additional || $(data.element).parent().data("otherInfo");
                                if (additional != null && additional.length > 0) {
                                    $remark.show();

                                    $remark.attr("data-content", additional);
                                }
                                else {
                                    $remark.hide();
                                }
                            }
                            else {
                                $remark.attr("data-content", "");
                                $remark.hide();
                            }
                        }
                        return data.text;
                    };
                    oSettings.dropdownAutoWidth = "true";
                }

                if (!$element.hasClass("skip-select2")) {
                    $element.select2(oSettings);
                    if (oSettings.otherInfo != null) {
                        if (oSettings.otherInfo.length > 0) {
                            $element.next().after('<a tabindex="0" title="' + (oSettings.otherInfoTitle || "Remark") + '"'
                                + ' role="button" data-toggle="popover" data-trigger="focus"  data-content="' + (oSettings.otherInfo.length > 0 ? oSettings.otherInfo : "not available") + '"  data-otherinfo="' + $element.attr("name") + '"'
                                + ' class = "ohterinforemark" >'
                                + ' <i class="fa fa-ellipsis-h" aria-hidden="true"></i></a> ');

                            if ($element.val() == null) {
                                $('[data-otherinfo="' + $element.attr("name") + '"]').hide();
                            }
                            $('[data-toggle="popover"]').popover({ trigger: 'focus' });
                        }
                    }
                }
                $element.css({ "width": "auto", "display": "block !important" });
                if (oSettings.dependent != undefined && oSettings.dependent) {
                    _fnSetDependent($element);
                    $element.on("change", function () {
                        _fnSetDependent($(this), true);
                    });
                }

                if (oSettings.hold && oSettings.hold !== null && oSettings.hold !== '') {
                    $element.on("change", function () {
                        var text = $(this).find("option:selected").map(function () {
                            return $(this).text();
                        }).get().join(',');
                        var options = $(this).data('plugginSelect2');
                        if (options && options.hold && options.hold !== null && options.hold != '') {
                            $(options.hold).val(text);
                        }
                    });
                }
            });
        }
    };
    this.fnInitTimePicker = function (modal) {
        var container = (modal && modal == true) ? _oSelectors.document.modal
            : _oSelectors.document.contentwrapper;
        if ($.fn.timepicker != undefined) {
            $(container).find(".timepicker").each(function () {
                //$(this).timepicker({
                //    showMeridian: false,
                //})
                $(this).timepicker({
                    showMeridian: false,
                    timeFormat: $(this).data("format") || 'H:i:s',
                    interval: 15,
                    dynamic: true,
                    dropdown: true,
                    scrollbar: true,
                });

                if ($(this).data("format") == undefined) {
                    $(this).inputmask("99:99:99");
                }
            });
        }
    }

    this.fnGetSwalSettings = function (element, data) {
        var options = $(this).data("swal") || $(element).data("swal") || data || {};
        return {
            title: options.title || 'Are you sure want to delete?',
            text: options.text || "You won't be able to revert this!",
            type: options.type || 'warning',
            showCancelButton: options.showCancelButton || true,
            confirmButtonColor: options.confirmButtonColor || '#3085d6',
            cancelButtonColor: options.cancelButtonColor || '#d33',
            confirmButtonText: options.confirmButtonText || 'Yes, delete it!',
            fnCallBackSuccess: options.fnCallBackSuccess || function () { }
        };
    };

    this.fnDeleteRow = function () {
        var $anchor = $(this);
        swal(fnGetSwalSettings($(this)))
            .then(function () {
                var table = $anchor.closest('table');
                var tbody = table.find('tbody');
                $anchor.parents('tr').remove();
                if (tbody.find('tr').length == 0) {
                    var colspan = table.find('thead > tr:last').find('th').length;
                    tbody.html('<tr class="empty-row"><td valign="top" colspan="' + colspan + '" class="dataTables_empty text-center">No data available in table</td ></tr>');
                }
            });
    };

    this.fnInitAjaxComponents = function (modal, wrapper) {
        var container = (modal && modal == true)
            ? _oSelectors.document.modal
            : _oSelectors.document.contentwrapper
        if (wrapper != null) {
            container = wrapper;
        }

        $(container).find('[data-val-required]:visible')
            .filter(":not('.skip-required')")
            .each(function () {
                var name = $(this).attr('name');
                var $label = $('label[for="' + name.replace('[', '_').replace(']', "_").replace('.', '_') + '"]');
                if ($label.length > 0) {
                    $label.find('.text-red').remove();
                    var required = $('<sup />', {
                        "class": 'text-red',
                        text: '*'
                    });
                    $label.append(required);
                }
            });
        if ($.isFunction($.fn.tooltip))
            $('[data-toggle="tooltip"]').tooltip();

        if (typeof CKEDITOR !== 'undefined') {
            $(container).find('.ck-editor').each(function () {
                var id = $(this).attr('id');
                var toolbar = $(this).attr('data-editor');
                if (CKEDITOR.instances[id]) {
                    CKEDITOR.instances[id].destroy();
                }
                if (toolbar)
                    CKEDITOR.replace(id, { toolbar: toolbar });
                else
                    CKEDITOR.replace(id);
            });
        }

        this.fnInitSelect2(modal, container);
        this.fnInitiCheck(modal, container);
        this.fnInitTimePicker(modal, container);
        if ($.fn.format != undefined) {
            $(".numericOnly").format({ precision: 0, autofix: true, allow_negative: false });
            $(".decimalOnly").format({ precision: 2, autofix: true, allow_negative: false });
            $(".numericOnly,.decimalOnly").not(".skip-blank").each(function () {
                $(this).val($(this).val().replace(".00", ""));
                if ($(this).val() === "0") {
                    $(this).val("");
                }
            });
            $('.onlyAlphanumeric').on('keypress', function (event) {
                var regex = new RegExp("^[a-zA-Z0-9 ]+$");
                var key = String.fromCharCode(!event.charCode ? event.which : event.charCode);
                if (!regex.test(key)) {
                    event.preventDefault();
                    return false;
                }
            });
        }
        if ($.fn.datepicker != undefined) {
            var newDate = new Date();
            $(".daymonthpicker").datepicker({ format: 'dd-mm', todayHighlight: true, autoclose: true });
            $(".datepicker").datepicker({ format: dateformat, todayHighlight: true, autoclose: true, startDate: this.fnTruncateDate(newDate) });
            $(".input-daterange.input-shortrange").datepicker({ format: dateformat, autoclose: true, startDate: $('.fromdate').text(), endDate: $('.todate').text() });
            $(".input-daterange").not("[data-fy-wise],[data-fullrange]").datepicker({ format: dateformat, autoclose: true, startDate: this.fnTruncateDate(newDate) });
            $("[data-fullrange]").datepicker({ format: dateformat, autoclose: true, todayHighlight: true });
            $("[data-fy-wise]").datepicker({ format: dateformat, autoclose: true, startDate: this.fnTruncateDate(newDate), endDate: this.getFyEndDate() });
            $(".dob").datepicker({ format: dateformat, todayHighlight: true, autoclose: true, endDate: currentDateWithGmt });
            $(".date-datepicker").datepicker({ format: dateformat, todayHighlight: true, autoclose: true });
        }

        $("[data-val-length-max]").each(function () {
            $(this).attr("maxlength", $(this).data("val-length-max"));
        });

        this.fnGetBadgesCount();
        this.fnDeleteMultipleImages();

        //$('.validity-index').eq(0).find('.removeContractValidity').eq(0).remove();

        if ($.isFunction($.fn.dirty)) {
            $('form').each(function () {
                $(this).dirty();
            });
        }

        $('[data-mode="password"]').attr('type', 'password');
    };

    this.initilize = (function () {
        this.fnAjaxView(window.location.href);
        $(document).off("click", "[data-navigateurl]").on("click", "[data-navigateurl]", function (event) {
            event.preventDefault();
            if (event.ctrlKey && event.type == "click") {
                window.open($(this).data('navigateurl'));
                return false;
            }
            else {
                var navigateUrl = $(this).attr("data-navigateurl");
                var wrapper = $(this).attr("data-navigateurl-wrapper");
                if (wrapper == undefined) {
                    $(_oSelectors.document.contentwrapper).html(overlayTemplate);
                }
                else {
                    $(wrapper).html(overlayTemplate);
                }

                fnAjaxView(navigateUrl, wrapper);

                if ($(this).data('notchangeurl') == undefined && !$(this).hasClass('data-notchangeurl')) {
                    fnChangeUrl(navigateUrl);
                }
            }
        });
        //window.onpopstate = function () {
        //    var navigateUrl = window.location.href;
        //    //$(_oSelectors.document.contentwrapper).html(overlayTemplate);
        //    fnAjaxView(navigateUrl);
        //}

        $(document).on("click", 'ul.sidebar-menu a', function () {
            fnHighlightMenu($(this));
        });

        $(document).on("click", '.activenexttab:submit', function (event) {
            var nextTab = $('.nav-tabs').hasClass('pull-right')
                ? $('.nav-tabs > .active').prev('li').find('a').attr("href")
                : $('.nav-tabs > .active').next('li').find('a').attr("href")
            $(this).parents('form').find(':hidden[name="nextview"]').remove();
            $(this).parents('form').append('<input name="nextview" type="hidden" value="' + nextTab + '" />');
        });

        //// Added By Sumeet Gupta
        $(document)
            .on('keydown', '.alpha', function (e) {
                var a = e.key;
                if (a.length == 1) return /[a-z]|\$|#|\*/i.test(a);
                return true;
            })
            .on('keydown', '.alphawithspace', function (e) {
                var a = e.key;
                if (a.length == 1) return /^(\w+\s?)*\s*$/i.test(a);
                return true;
            })
            .on('keydown', '.numeric', function (e) {
                var a = e.key;
                if (a.length == 1) return /[0-9]|\+|-/.test(a);
                return true;
            })
            .on('keydown', '.alphanumeric', function (e) {
                var a = e.key;
                if (a.length == 1) return /^[a-zA-Z0-9\-\s]+$/i.test(a);
                return true;
            })

        //// added by sumeet
        $(document).on("click", '.commandbtn:submit', function (event) {
            $(this).parents('form').find(':hidden[name="commandbutton"]').remove();
            $(this).parents('form').append('<input name="commandbutton" type="hidden" value="' + event.target.name + '" />');

            if ($(this).parents('form').valid()) {
                var type = $(event.target).text();
                $(this).parents('form').find(':hidden[name="ButtonActionType"]').remove();
                $(this).parents('form').append('<input name="ButtonActionType" type="hidden" value="' + type + '" />');
            }
        });

        $(document).on("click", '.draftcommanbtn:submit', function (event) {
            $(this).parents('form').find(':hidden[name="draftcommanbtn"]').remove();
            $(this).parents('form').append('<input name="draftcommanbtn" type="hidden" value="' + event.target.name + '" />');

            if ($(this).parents('form').valid()) {
                var type = $(event.target).text();
                $(this).parents('form').find(':hidden[name$="BookingStatus"]').remove();
                $(this).parents('form').append('<input name="BookingStatus" type="hidden" value="' + type + '" />');
            }
        });

        //// added by sumeet
        ////$(document).on("click", '.confirmisleiousbooking', function (event) {
        ////    debugger
        ////    $(this).closest("tr.itemmaster-row").find(':hidden[name="commandbutton"]').remove();
        ////    $(this).closest("tr.itemmaster-row").append('<input name="commandbutton" type="hidden" value="' + event.target.name + '" />');
        ////        var type = $(event.target).text();
        ////        $(this).closest("tr.itemmaster-row").find(':hidden[name="BookingStatus"]').remove();
        ////        $(this).closest("tr.itemmaster-row td:eq(0)").append('<input name="BookingStatus" type="hidden" value="' + type + '" />');

        ////});

        //// added by sumeet
        $(document).on("click", '.addneweffectivedate', function (event) {
            if ($(this).parents('form').valid()) {
                $(this).parents('form').find(':hidden[name="AddNewEffectiveDate"]').remove();
                $(this).parents('form').append('<input name="AddNewEffectiveDate" type="hidden" value="AddNewEffectiveDate" />');
                $('.effectivedate-message').text('Please Change effective Date and add new cost');
            }
        });

        window.captcha = true;
        $(document).on("submit", 'form', function (event) {           
            event.preventDefault();
            var $form = $(this);
            if (!$form.hasClass("skip-global-submit")) {
                if ($form.is('form')) {
                    if ($form.valid() && captcha) {
                        captcha = false;
                        showWaitProcess({ Text: "Please Wait..." });
                        var $modal = $form.parents('.modal');
                        $(document).data("form-data", $form.data());
                        $.ajax({
                            url: $form.attr('action') || location.pathname,
                            dataType: $form.data('datatype') || 'json',
                            type: 'post',
                            data: new FormData($form[0]),
                            enctype: 'multipart/form-data',
                            async: false,
                            cache: false,
                            contentType: false,
                            processData: false,
                            error: function (xhr, error, thrown) {
                                captcha = true;
                                var errors = JSON.parse(xhr.responseText);
                                $.each(errors, function (i, obj) {
                                    var $span = $('span[data-valmsg-for="' + obj.key + '"]');
                                    if ($span.length > 0) {
                                        $span.addClass("field-validation-error").html('<span for="' + obj.key + '" class="field-validation-error">' + obj.error + '</span>');
                                        $span.attr("data-valmsg-replace", "false");
                                    }
                                    else {
                                        fnShowMessage(obj.error, 'error');
                                    }
                                })
                                setTimeout(function () {
                                    $("[data-valmsg-replace]").each(function () {
                                        $(this).attr("data-valmsg-replace", "true");
                                    })
                                }, 3000);
                                hideWaitProcess();
                            },
                            success: function (json) {
                                if ($modal.length > 0) {
                                    setTimeout(function () {
                                        $modal.modal("hide");
                                    }, 1000)
                                }
                                hideWaitProcess();
                                var formData = $(document).data('form-data');
                                if (!(json instanceof Object) && json.indexOf("status") != -1 && json.indexOf("alert") != -1) {
                                    try {
                                        json = JSON.parse(json);
                                    } catch (e) {
                                    }
                                }
                                if (json instanceof Object) {
                                    if (json.alert !== null && json.alert.message != null) {
                                        fnShowMessage(json.alert.message, json.alert.type);
                                    }

                                    if (json.navigateUrl !== null && json.navigateUrl !== '') {
                                        
                                        if (json.isajaxurl) {
                                            if (json.header !== null && json.header !== '') {
                                                fnAjaxView(json.navigateUrl, null, null, null, json.header);
                                            }
                                            else {
                                                fnAjaxView(json.navigateUrl);
                                            }
                                            fnChangeUrl(json.navigateUrl);
                                        }
                                        else {
                                            window.location = json.navigateUrl;
                                        }
                                    }
                                    if (json.modal && (json.modal || '').length > 0) {
                                        $(json.modal).modal('hide');
                                    }

                                    if (json.autoClick && (json.autoClick || '').length > 0 && $(json.autoClick).length > 0) {
                                        $(json.autoClick).click();
                                    }

                                    if (json.fnName != null) {
                                        if (json.jsonData != null) {
                                            var jdata = JSON.parse(json.jsonData);
                                            window[json.fnName](jdata);
                                        }
                                        else {
                                            window[json.fnName]();
                                        }
                                    }
                                }

                                if (!(json instanceof Object) && formData) {
                                    fnAppedHtml(json);
                                }
                                if (json.fnName == null) {
                                    openServiceEmpty();
                                }

                            }
                        });
                    }
                }
            }
        });

        $(document).on('show.bs.tab', 'a[data-toggle="tab"][data-href]', function (e) {
            $(".mobPosDropdown").remove();
            if (!$(this).parent().hasClass('active')) {
                const url = $(this).data("href");
                const target = $(this).attr("href");
                const currentTab = $(this);
                const ul = $(this).parents('ul');
                const li = $(this).closest('li');
                if (url && url != '' && target.indexOf("#") != -1 && !li.hasClass("disabled")) {
                    $(target).html(overlayTemplate);
                    var data = {};
                    ul.next('.tab-content').find(".active").html('')
                    var form = ul.closest('div').children('form.hide');
                    if (form.length) { data = form.serialize(); }
                    $.ajax({
                        url: url,
                        data: data,
                        method: 'get',
                        dataType: 'html',
                        error: function (xhr, request, error) { },
                        success: function (html) {
                            $(target).html(html);
                            currentTab.tab('show');
                            if ($(currentTab).data("href-replace") && $(currentTab).data("href-replace") === true) {
                                var replaceUrl = $(currentTab).data("href") || '';

                                if ($(currentTab).data("virtual-url")) {
                                    replaceUrl = $(currentTab).data("virtual-url");
                                }

                                window.history.pushState({}, document.title, window.location.origin +
                                    replaceUrl +
                                    $(currentTab).attr('href'));
                            } else {
                                history.pushState({}, "", $(currentTab).attr('href'));
                            }

                            fnInitValidator();
                            fnInitAjaxComponents();
                            if ($.fn.tooltip) {
                                $('[title]').attr('data-placement', 'auto').tooltip();
                            }
                        },
                    });
                }
            }
        });

        $(document).on("change", ':file.preview', function () {
            if ($(this).data('target')) {
                fnPreview($(this)[0], $(this).data('target'), $(this));
                //if ($.fn.masonry != undefined) {
                //    $('.preview-thumbs').masonry({ itemSelector: ".preview-thumbs > div" });
                //}
            }
        });

        $(document).off('.bs.tab.data-api');

        $(document).on("click", '[data-toggle="tab"]', function (e) {
            e.preventDefault()
            if ($(this).closest('li').hasClass("disabled")) {
                return false;
            }
            $(this).tab('show')
        });

        $(document).on('focus', ':input', function () {
            var $form = $(this).parents('form');
            if ($form.attr('autocomplete') == undefined) {
                $form.attr('autocomplete', 'off');
            }
        });

        $(document).on('click', 'a.delete-tr', fnDeleteRow);
    })();

    //window.addEventListener('popstate', function (event) {
    //    //if (location.href.indexOf("#") > -1) {
    //    if (localStorage.prevUrl.length > 0) {
    //        document.title = $(_oSelectors.document.contentwrapper)
    //            .find(_oSelectors.document.contentTitle).html() || '' + " | " + applicationName;
    //        window.history.pushState({}, "", localStorage.prevUrl);
    //        fnAjaxView(localStorage.prevUrl);
    //    }
    //    //}
    //}, false);

    window.onload = function () {
        //if ($(".main-header").length>1) {
        //    location.reload();
        //}
        if (typeof history.pushState === "function") {
            //history.pushState("av", null, null);
            window.onpopstate = function () {
                //history.pushState('newjibberish', null, null);
                var navigateUrl = window.location.href;
                //$(_oSelectors.document.contentwrapper).html(overlayTemplate);
                fnAjaxView(navigateUrl);
            };
        }
        else {
            var ignoreHashChange = true;
            window.onhashchange = function () {
                if (!ignoreHashChange) {
                    ignoreHashChange = true;
                    window.location.hash = Math.random();
                    //var navigateUrl = window.location.href;
                    ////$(_oSelectors.document.contentwrapper).html(overlayTemplate);
                    //fnAjaxView(navigateUrl);
                }
                else {
                    ignoreHashChange = false;
                }
            };
        }
    }

    window.digits = (function (n, t, i, r) {
        var u, e = !1, f, o, s, h; return (window.event ? (u = t.keyCode, e = window.event.ctrlKey) : t.which && (u = t.which, e = t.ctrlKey), isNaN(u)) ? !0 : (f = String.fromCharCode(u), u == 8 || e) ? !0 : (o = /\d/, s = r ? f == "-" && n.value.indexOf("-") == -1 : !1, h = i ? f == "." && n.value.indexOf(".") == -1 : !1, s || h || o.test(f))
    });
    $(document).on("keypress", ".decimalvalues", function (event) {
        return digits(this, event, true, false);
    })

    $(document).on("keypress", ".numericvalues", function (event) {
        return digits(this, event, false, false);
    })
    $(document).on("keypress", ".negativewithdecimalvalues", function (event) {
        return digits(this, event, true, true);
    })
    $(document).on("keypress", ".negativenumber", function (event) {
        return digits(this, event, false, true);
    })

    //$('[data-navigateurl]').click(function (e) {
    //    if (e.ctrlKey && e.type == "click") {
    //        window.open($(this).data('navigateurl'));
    //        return false;
    //    }
    //});
})()

function excelImport() {
    this.url = { import: "/Excel/Import", template: "/Excel/DownloadTemplate" };
    this.modal = 'div[data-import="excel-modal"]';
    this.excelmodel = 'div[data-import="excel-modal-table"]';
    this.on = { modal: 'a[data-import="excel-modal"]', import: '[data-import="excel"]' };
    this.init = (function () {
        var _self = this;
        $(document).off("click", _self.on.modal).on("click", _self.on.modal, function (e) {
            e.preventDefault();
            var $anchor = $(this);
            $(_self.modal).find('.modal-body').html(overlayTemplate);
            $(_self.modal).modal({ keyboard: true, backdrop: 'static' }).show();            
            $.ajax({
                url: _self.url.import,
                type: 'get',
                success: function (htmlSource) {                   
                    var filename = $anchor.data('template');
                    var link2 = $anchor.data('link2');
                    $(_self.modal).find('.modal-body').html('').html(htmlSource)
                    $(_self.modal).find('.modal-body').find('[data-import="excel"]').data("url", $anchor.data('url'))
                    $(_self.modal).find('.modal-body').find('.singleSheet').attr("href", _self.url.template + "?excelPath=" + filename);
                    $(_self.modal).find('.modal-body').find('.doubleSheet').attr("href", _self.url.template + "?excelPath=" + link2);
                    if (link2 == undefined || link2 == "") {
                        $(_self.modal).find('.modal-body').find('.doubleSheet').remove();
                        $(_self.modal).find('.modal-body').find('.download-devider').remove();
                    }
                },
                error: function (response) { },
            });
        });

        $(document).off("click", _self.on.import).on("click", _self.on.import, function (e) {
            var $anchor = $(this);
            var filename = $("#FileName").val();
            if ((filename != "")) {
                var frmData = { filename: filename };
                $(".divMessage").removeClass('show').hide();
                showWaitProcess();
                $.ajax({
                    url: $anchor.data("url"),
                    data: frmData,
                    type: 'get',
                    success: function (json) {
                        hideWaitProcess();
                        if (!json.status && json.htmlTables instanceof Object && Object.keys(json.htmlTables).length > 0) {
                            $(_self.modal).modal("hide")
                            _self.excelResponse(json);
                        }
                        else if (json.status === true) {
                            $(_self.modal).modal("hide")
                            swal("", json.message, "success");
                            //window.location = window.location.href;
                            fnAjaxView(json.navigateUrl);
                        }
                        else {
                            $(".divMessage").addClass('show').html(json.message).show();
                        }
                    }.bind(_self),
                    error: function (response) { hideWaitProcess(); },
                });
                return true;
            }
            else {
                $(".divMessage").show();
                return false;
            }
        });
        $(document).off("click", "[data-close-modal]")
            .on("click", "[data-close-modal]", function () {
                var modal = $(this).data("close-modal");
                if (modal != undefined && $(modal).length > 0) {
                    $(modal).modal('hide');
                    $(".modal-backdrop").css({ "z-index": '' });
                }
            });
    })();

    this.excelResponse = function (json) {
        var _self = this;
        if (json instanceof Object && json.htmlTables instanceof Object) {
            if (!json.status) {
                $(_self.excelmodel).find('.dvContents').html(overlayTemplate);
                $(_self.excelmodel).modal({ keyboard: true, backdrop: 'static' }).show();

                var navTabs = '', navContent = '';
                navTabs += '<ul class="nav nav-tabs">';
                var html = [];

                for (var i = 0; i < Object.keys(json.htmlTables).length; i++) {
                    var key = Object.keys(json.htmlTables)[i];
                    var value = json.htmlTables[key];
                    if (value != undefined && value.length > 0) {
                        html.push({ Name: key, Html: value });
                    }
                }

                for (var counter = 0; counter < html.length; counter++) {
                    navTabs += '<li class="' + (counter == 0 ? "active" : "") + '">';
                    navTabs += '<a data- toggle="tab" href= "#' + html[counter].Name + '" >' + html[counter].Name + '</a >';
                    navTabs += '</li >';
                }
                navTabs += '</ul>';
                navContent += '<div class="tab-content excel-response">';

                for (var index = 0; index < html.length; index++) {
                    navContent += '<div id="' + html[index].Name + '" class="tab-pane fade ' + (index == 0 ? "in active" : "") + '">';
                    navContent += '<div>' + html[index].Html + '</div>';
                    navContent += '</div>';
                }
                navContent += '</div>';
                $(_self.excelmodel).find('.dvContents').html('<div>' + navTabs + navContent + '</div>');

                // Select all tabs
                $('.nav-tabs a').click(function () {
                    $(this).tab('show');
                });
            }
        }
    }
};
$("[data-fy-option]").on("click", function () {
    var $ele = $(this);
    var id = $ele.data("id");
    $.post("/shared/setfy/" + id, function (json) {
        if (id != null && id != "" && id != "0") {
            $("[data-assy-dis]").html($ele.data("assy"));
            $ele.closest('ul').find('li').removeClass('active')
            $ele.closest('li').addClass('active');
            fnAjaxView(location.href);
        }
    });
})