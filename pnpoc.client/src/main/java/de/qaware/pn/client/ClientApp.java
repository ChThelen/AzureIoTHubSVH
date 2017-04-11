package de.qaware.pn.client;

import com.microsoft.azure.sdk.iot.device.*;

import java.io.IOException;
import java.net.URISyntaxException;

public class ClientApp {
    private static IotHubClientProtocol protocol = IotHubClientProtocol.MQTT;

    private static class AppMessageCallback implements MessageCallback {
        public IotHubMessageResult execute(Message msg, Object context) {
            System.out.println("Received message from hub: "
                    + new String(msg.getBytes(), Message.DEFAULT_IOTHUB_MESSAGE_CHARSET));

            return IotHubMessageResult.COMPLETE;
        }
    }

    public static void main(String[] args) throws IOException, URISyntaxException {
        String connString = "HostName=SVH-Hub.azure-devices.net;DeviceId=mydevice456;SharedAccessKey=OuwhNbfmQMlOjCducT8SXg==";
        DeviceClient client = new DeviceClient(connString, protocol);

        MessageCallback callback = new AppMessageCallback();
        client.setMessageCallback(callback, null);
        client.open();

        System.out.println("Press ENTER to exit.");
        System.in.read();
        client.close();
    }
}
