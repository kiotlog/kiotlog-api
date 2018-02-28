var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var Kiotlog;
(function (Kiotlog) {
    const { customElement, property, observe, query, computed } = Polymer.decorators;
    Polymer.setPassiveTouchGestures(true);
    let KtlgApp = KtlgApp_1 = class KtlgApp extends Polymer.Element {
        constructor() {
            super(...arguments);
            this.apiUrl = 'http://127.0.0.1:8888/';
        }
        _routePageChanged(page) {
            this.page = page || 'dashboard';
            if (!(this.drawer).persistent) {
                this.drawer.close();
            }
        }
        _pageChanged(page) {
            const resolvedPageUrl = this.resolveUrl('ktlg-' + page + '.html');
            Polymer.importHref(resolvedPageUrl, null, this._showPage404.bind(this), true);
        }
        _showPage404() {
            this.page = 'view404';
        }
    };
    __decorate([
        property({ type: String, reflectToAttribute: true, observer: KtlgApp_1.prototype._pageChanged })
    ], KtlgApp.prototype, "page", void 0);
    __decorate([
        property({ type: Object })
    ], KtlgApp.prototype, "routeData", void 0);
    __decorate([
        property({ type: Object })
    ], KtlgApp.prototype, "subroute", void 0);
    __decorate([
        property({ type: String })
    ], KtlgApp.prototype, "rootPath", void 0);
    __decorate([
        property({ type: String })
    ], KtlgApp.prototype, "apiUrl", void 0);
    __decorate([
        query('#drawer')
    ], KtlgApp.prototype, "drawer", void 0);
    __decorate([
        observe('routeData.page')
    ], KtlgApp.prototype, "_routePageChanged", null);
    KtlgApp = KtlgApp_1 = __decorate([
        customElement('ktlg-app')
    ], KtlgApp);
    Kiotlog.KtlgApp = KtlgApp;
    var KtlgApp_1;
})(Kiotlog || (Kiotlog = {}));
//# sourceMappingURL=ktlg-app.js.map