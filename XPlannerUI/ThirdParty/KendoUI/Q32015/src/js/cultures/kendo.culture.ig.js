/*
* Kendo UI Beta v2015.3.916 (http://www.telerik.com/kendo-ui)
* Copyright 2015 Telerik AD. All rights reserved.
*
* Kendo UI Beta license terms available at
* http://www.telerik.com/purchase/license-agreement/kendo-ui-beta.aspx
*/

(function(f, define){
    define([], f);
})(function(){

(function( window, undefined ) {
    var kendo = window.kendo || (window.kendo = { cultures: {} });
    kendo.cultures["ig"] = {
        name: "ig",
        numberFormat: {
            pattern: ["-n"],
            decimals: 2,
            ",": ",",
            ".": ".",
            groupSize: [3],
            percent: {
                pattern: ["-n %","n %"],
                decimals: 2,
                ",": ",",
                ".": ".",
                groupSize: [3],
                symbol: "%"
            },
            currency: {
                pattern: ["$-n","$ n"],
                decimals: 2,
                ",": ",",
                ".": ".",
                groupSize: [3],
                symbol: "₦"
            }
        },
        calendars: {
            standard: {
                days: {
                    names: ["Sọnde","Mọnde","Tuzde","Wednesde","Tọsde","Fraịde","Satọde"],
                    namesAbbr: ["Sọn","Mọn","Tuz","Ojo","Tọs","Fra","Sat"],
                    namesShort: ["Sọ","Mọ","Tu","We","Tọs","Fra","Sa"]
                },
                months: {
                    names: ["Jenụwarị","Febụwarị","Machị","Eprelu","Mey","Juun","Julaị","Ọgọst","Septemba","Ọcktọba","Nọvemba","Disemba"],
                    namesAbbr: ["Jen","Feb","Mac","Epr","Mey","Jun","Jul","Ọgọ","Sep","Ọkt","Nọv","Dis"]
                },
                AM: ["Ụtụtụ","ụtụtụ","ỤTỤTỤ"],
                PM: ["Ehihie","ehihie","EHIHIE"],
                patterns: {
                    d: "d/M/yyyy",
                    D: "dddd, MMMM dd, yyyy",
                    F: "dddd, MMMM dd, yyyy h.mm.ss tt",
                    g: "d/M/yyyy h.mm tt",
                    G: "d/M/yyyy h.mm.ss tt",
                    m: "d MMMM",
                    M: "d MMMM",
                    s: "yyyy'-'MM'-'dd'T'HH':'mm':'ss",
                    t: "h.mm tt",
                    T: "h.mm.ss tt",
                    u: "yyyy'-'MM'-'dd HH':'mm':'ss'Z'",
                    y: "MMMM, yyyy",
                    Y: "MMMM, yyyy"
                },
                "/": "/",
                ":": ".",
                firstDay: 0
            }
        }
    }
})(this);


return window.kendo;

}, typeof define == 'function' && define.amd ? define : function(_, f){ f(); });