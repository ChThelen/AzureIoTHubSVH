package de.qaware.pn.client;

import com.microsoft.azure.sdk.iot.device.*;
import com.microsoft.azure.sdk.iot.device.DeviceTwin.Property;
import com.microsoft.azure.sdk.iot.device.DeviceTwin.PropertyCallBack;

import java.io.IOException;
import java.net.URISyntaxException;
import java.util.HashSet;

public class ClientApp {
    private static IotHubClientProtocol protocol = IotHubClientProtocol.MQTT;

    private static class AppMessageCallback implements MessageCallback {
        public IotHubMessageResult execute(Message msg, Object context) {
            System.out.println("Received message from hub: "
                    + new String(msg.getBytes(), Message.DEFAULT_IOTHUB_MESSAGE_CHARSET));

            return IotHubMessageResult.COMPLETE;
        }
    }

    private static class DeviceTwinCallback implements IotHubEventCallback {

        @Override
        public void execute(IotHubStatusCode responseStatus, Object callbackContext) {
            System.out.println("DeviceTwinCallback called with " + responseStatus.toString() + ", callbackContext " + callbackContext.toString());
        }
    }

    private static class PropertyCallback implements PropertyCallBack {

        @Override
        public void PropertyCall(Object propertyKey, Object propertyValue, Object context) {
            System.out.println("Found prop:");
            System.out.println("propertyKey = [" + propertyKey + "], propertyValue = [" + propertyValue + "], context = [" + context + "]");
        }
    }

    public static void main(String[] args) throws IOException, URISyntaxException {
        String sas = "SharedAccessSignature sr=SVH-Hub.azure-devices.net%2Fdevices%2Fmydevice457&sig=SQ8lxcRrBhfPEeaKmHakGqayjauM1gcSXj97vuRbmkg%3D&se=1491909042";
        String connString = "HostName=SVH-Hub.azure-devices.net;DeviceId=mydevice457;SharedAccessSignature=" + sas;
//        String connString = "HostName=SVH-Hub.azure-devices.net;DeviceId=mydevice456;SharedAccessKey=OuwhNbfmQMlOjCducT8SXg==";
//        String connString = "HostName=SVH-Hub.azure-devices.net;DeviceId=mydevice458;SharedAccessKey=vkk4Q0y9JUSaF4vxVsWG/Q==";
        DeviceClient client = new DeviceClient(connString, protocol);

        MessageCallback callback = new AppMessageCallback();
        client.setMessageCallback(callback, null);
        client.open();
        Property property = new Property("MyProp", "works");
        HashSet<Property> propSet = new HashSet<>();
        propSet.add(property);
        client.startDeviceTwin(new DeviceTwinCallback(), "testContext", new PropertyCallback(), "propContext");
        client.sendReportedProperties(propSet);

        System.out.println("Press ENTER to exit.");
        System.in.read();
        client.close();
    }
}
