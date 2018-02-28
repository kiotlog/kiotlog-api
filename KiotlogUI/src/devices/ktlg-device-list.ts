/// <reference path="../../bower_components/polymer/types/lib/utils/render-status.d.ts" />
/// <reference path="../../bower_components/iron-ajax/iron-ajax.d.ts" />

namespace Kiotlog {

    const {customElement, property, computed, query} = Polymer.decorators;

    @customElement('ktlg-device-list')
    export class KtlgDeviceList extends Polymer.Element {
        @property({type: String})
        apiUrl: string; // TODO: should set a default value?

        @computed<KtlgDeviceList>('apiUrl')
        @property({type: String})
        private get _computedUrl() {
            let _url = this.apiUrl.replace(/\/$/, "") + "/devices";

            // console.log('computing iron-ajax URL: '+_url);

            return _url;
        }

        @property({type: Array, notify: true})
        stations: any[];

        @query('#devicesAjax')
        ajax: IronAjaxElement; // TODO: Add type when available

        private _handleResponse(response: any) {
            // console.log(response.detail.response);
            this.stations = response.detail.response;
        }

        constructor() {
            super();
            // When possible, use afterNextRender to defer non-critical
            // work until after first paint.
            Polymer.RenderStatus.afterNextRender(this, function() {
                this.ajax.generateRequest();
            });
        }
    }
}