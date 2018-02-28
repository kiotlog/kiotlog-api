/// <reference path="../../bower_components/polymer/types/lib/utils/render-status.d.ts" />
/// <reference path="../../bower_components/iron-ajax/iron-ajax.d.ts" />

namespace Kiotlog {

    const {customElement, property, computed, query} = Polymer.decorators;

    @customElement('ktlg-sensor-types-list')
    export class KtlgSensorTypesList extends Polymer.Element {
        @property({type: String})
        apiUrl: string; // TODO: should set a default value?

        @computed<KtlgSensorTypesList>('apiUrl')
        @property({type: String})
        private get _computedUrl() {
            let _url = this.apiUrl.replace(/\/$/, "") + "/sensortypes";

            // console.log('computing iron-ajax URL: '+_url);

            return _url;
        }

        @property({type: Array, notify: true})
        sensortypes: any[];

        @query('#sensorTypesAjax')
        ajax: IronAjaxElement;

        private _handleResponse(response: any) {
            // console.log(response.detail.response);
            this.sensortypes = response.detail.response;
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