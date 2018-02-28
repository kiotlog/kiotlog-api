var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var Kiotlog;
(function (Kiotlog) {
    const { customElement, property, computed, query } = Polymer.decorators;
    let KtlgSensorTypesList = class KtlgSensorTypesList extends Polymer.Element {
        constructor() {
            super();
            Polymer.RenderStatus.afterNextRender(this, function () {
                this.ajax.generateRequest();
            });
        }
        get _computedUrl() {
            let _url = this.apiUrl.replace(/\/$/, "") + "/sensortypes";
            return _url;
        }
        _handleResponse(response) {
            this.sensortypes = response.detail.response;
        }
    };
    __decorate([
        property({ type: String })
    ], KtlgSensorTypesList.prototype, "apiUrl", void 0);
    __decorate([
        computed('apiUrl'),
        property({ type: String })
    ], KtlgSensorTypesList.prototype, "_computedUrl", null);
    __decorate([
        property({ type: Array, notify: true })
    ], KtlgSensorTypesList.prototype, "sensortypes", void 0);
    __decorate([
        query('#sensorTypesAjax')
    ], KtlgSensorTypesList.prototype, "ajax", void 0);
    KtlgSensorTypesList = __decorate([
        customElement('ktlg-sensor-types-list')
    ], KtlgSensorTypesList);
    Kiotlog.KtlgSensorTypesList = KtlgSensorTypesList;
})(Kiotlog || (Kiotlog = {}));
//# sourceMappingURL=ktlg-sensor-types-list.js.map