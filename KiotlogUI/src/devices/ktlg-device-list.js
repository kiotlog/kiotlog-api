var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var Kiotlog;
(function (Kiotlog) {
    const { customElement, property, computed, query } = Polymer.decorators;
    let KtlgDeviceList = class KtlgDeviceList extends Polymer.Element {
        constructor() {
            super();
            Polymer.RenderStatus.afterNextRender(this, function () {
                this.ajax.generateRequest();
            });
        }
        get _computedUrl() {
            let _url = this.apiUrl.replace(/\/$/, "") + "/devices";
            return _url;
        }
        _handleResponse(response) {
            this.stations = response.detail.response;
        }
    };
    __decorate([
        property({ type: String })
    ], KtlgDeviceList.prototype, "apiUrl", void 0);
    __decorate([
        computed('apiUrl'),
        property({ type: String })
    ], KtlgDeviceList.prototype, "_computedUrl", null);
    __decorate([
        property({ type: Array, notify: true })
    ], KtlgDeviceList.prototype, "stations", void 0);
    __decorate([
        query('#devicesAjax')
    ], KtlgDeviceList.prototype, "ajax", void 0);
    KtlgDeviceList = __decorate([
        customElement('ktlg-device-list')
    ], KtlgDeviceList);
    Kiotlog.KtlgDeviceList = KtlgDeviceList;
})(Kiotlog || (Kiotlog = {}));
//# sourceMappingURL=ktlg-device-list.js.map